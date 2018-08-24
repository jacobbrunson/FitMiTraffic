﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Dynamics;

namespace TestGame
{
	class RayDraw
	{
		public Vector2 P1;
		public Vector2 P2;
		public double SpawnTime;

		public RayDraw(Vector2 p1, Vector2 p2, double spawnTime)
		{
			P1 = p1;
			P2 = p2;
			SpawnTime = spawnTime;
		}
	}

	class TrafficManager
	{
		List<Car> cars = new List<Car>();
		List<RayDraw> rays = new List<RayDraw>();
		public int Interval;

		private World World;
		private long LastSpawnMillis;
		private ContentManager Content;
		private int NumLanes;
		private float LaneWidth;

		public TrafficManager(ContentManager content, World world, int interval, int numLanes, float laneWidth)
		{
			World = world;
			Interval = interval;
			Content = content;
			NumLanes = numLanes;
			LaneWidth = laneWidth;

			LastSpawnMillis = -Interval;
		}

		public void Update(GameTime gameTime, float playerY)
		{
			Random random = new Random();

			if (gameTime.TotalGameTime.TotalMilliseconds - LastSpawnMillis >= Interval)
			{
				LastSpawnMillis = (long)gameTime.TotalGameTime.TotalMilliseconds;

				var type = CarType.RANDOM;
				var texture = Content.Load<Texture2D>(type.TextureName);

				int lane = 0;
				float posX = 0, posY = 0;

				var foundSpawnPos = true;
				do
				{
					lane = random.Next(0, NumLanes);
					var lanePos = LaneWidth * (lane - NumLanes / 2) + LaneWidth / 2;

					var wiggleRoom = Math.Max(0, LaneWidth - type.Width - 0.1f);
					var laneOffset = (float)random.NextDouble() * wiggleRoom - wiggleRoom / 2;

					posX = lanePos + laneOffset;
					posY = playerY + (float)random.NextDouble() * 30 + 20;

					foundSpawnPos = true;
					foreach (Car c in cars)
					{
						Console.WriteLine(c.Position.Y - posY);
						if (c.Lane == lane && Math.Abs(c.Position.Y - posY) < Math.Max(c.BodySize.Y, type.Height) + 1)
						{
							foundSpawnPos = false;
							break;
						}
					}
				} while (!foundSpawnPos);

				float speed = (float)random.NextDouble() * 7.0f + (NumLanes-lane) * 3.0f;

				Car car = new Car(type, World, texture, speed);
				car.Position = new Vector2(posX, posY);
				car.Lane = lane;
				
				cars.Add(car);
			}

			float rayDist = 3f;
			foreach (Car car in cars)
			{
				Vector2 p1 = new Vector2(car.Position.X, car.Position.Y + car.BodySize.Y/2);
				Vector2 p2 = p1 + Vector2.UnitY * rayDist;

				rays.Add(new RayDraw(p1, p2, gameTime.TotalGameTime.TotalSeconds));
				List<Fixture> hits = World.RayCast(p1, p2);

				//anticipated collision
				if (hits.Count > 0)
				{
					car.Velocity = new Vector2(0, Math.Min(car.Velocity.Y, hits[0].Body.LinearVelocity.Y - 1));
				}
			}

			//Despawn cars we left in the dust
			for (int i = cars.Count-1; i >= 0; i--)
			{
				if (cars[i].Body.Position.Y < playerY - 10)
				{
					World.Remove(cars[i].Body);
					cars.Remove(cars[i]);
				}
			}

			foreach (Car car in cars)
			{
				car.Update(gameTime);
			}

			//rays.RemoveAll(ray => gameTime.TotalGameTime.TotalSeconds >= ray.SpawnTime + 0.25f);
		}

		public void RenderTraffic(SpriteBatch spriteBatch)
		{
			foreach (Car car in cars)
			{
				car.Render(spriteBatch);
			}

			foreach (RayDraw ray in rays)
			{
				Game1.DebugView.BeginCustomDraw(Game1.cameraEffect.Projection, Game1.cameraEffect.View);
				Game1.DebugView.DrawPoint(ray.P1, .25f, new Color(0.4f, 0.9f, 0.4f));

				Game1.DebugView.DrawSegment(ray.P2, ray.P1, new Color(0.8f, 0.8f, 0.8f));

				Game1.DebugView.EndCustomDraw();
			}
			rays.RemoveAll(ray => true);
		}

	}
}
