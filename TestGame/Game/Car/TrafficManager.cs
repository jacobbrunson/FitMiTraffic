using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Dynamics;

namespace FitMiTraffic
{
	class TrafficManager
	{
		List<Car> cars = new List<Car>();
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
				var spawnAttempts = 0;
				do
				{
					spawnAttempts += 1;

					lane = random.Next(0, NumLanes);
					var lanePos = LaneWidth * (lane - NumLanes / 2) + LaneWidth / 2;

					var wiggleRoom = Math.Max(0, LaneWidth - type.Width - 0.1f);
					var laneOffset = (float)random.NextDouble() * wiggleRoom - wiggleRoom / 2;

					posX = lanePos + laneOffset;
					posY = playerY + (float)random.NextDouble() * 30 + 20;

					foundSpawnPos = true;
					foreach (Car c in cars)
					{
						if (c.Lane == lane && Math.Abs(c.Position.Y - posY) < Math.Max(c.BodySize.Y, type.Height) + 1)
						{
							Console.WriteLine("bad car spawn position; retrying");
							foundSpawnPos = false;
							break;
						}
					}
				} while (!foundSpawnPos && spawnAttempts < 5);

				if (foundSpawnPos) {
					float speed = (float)random.NextDouble() * 5.0f + (NumLanes - lane + 1) * 2.0f;

					Car car = new Car(type, World, texture, speed);
					car.Position = new Vector2(posX, posY);
					car.Lane = lane;

					cars.Add(car);
				}
			}

			float rayDist = 3f;
			foreach (Car car in cars)
			{
				Vector2 p1 = new Vector2(car.Position.X, car.Position.Y + car.BodySize.Y/2);
				Vector2 p2 = p1 + Vector2.UnitY * rayDist;

				//rays.Add(new RayDraw(p1, p2, gameTime.TotalGameTime.TotalSeconds));
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
				Body b = car.AnticipateCollision(3);

				if (b != null)
				{
					car.Velocity = new Vector2(0, Math.Min(car.Velocity.Y, b.LinearVelocity.Y - 1));
				}

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

			/*foreach (RayDraw ray in rays)
			{
				
			}
			rays.RemoveAll(ray => true);*/
		}

	}
}
