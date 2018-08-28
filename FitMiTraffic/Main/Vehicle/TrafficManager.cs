using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitMiTraffic.Main.Environment;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Dynamics;

namespace FitMiTraffic.Main.Vehicle
{
	class TrafficManager
	{
		public List<Car> cars = new List<Car>();
		public int Interval;

		private World World;
		private long LastSpawnMillis;
		private ContentManager Content;
		private int NumLanes;
		public float LaneWidth;

		private float LastOnRamp = -1;
		private float LastOnRampSeconds; 

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

				CarType type;

				int lane = 0;
				float posX = 0, posY = 0;

				var foundSpawnPos = true;
				var spawnAttempts = 0;
				do
				{
					type = CarType.RANDOM;
					spawnAttempts += 1;

					lane = random.Next(0, NumLanes);
					var lanePos = Road.GetCenterOfLane(lane);

					var wiggleRoom = Math.Max(0, LaneWidth - type.Width - 0.1f);
					var laneOffset = (float)random.NextDouble() * wiggleRoom - wiggleRoom / 2;

					posX = lanePos + laneOffset;
					posY = playerY + (float)random.NextDouble() * 20 + 15;

					foundSpawnPos = true;
					foreach (Car c in cars)
					{
						if ((c.Lane == lane || c.Lane == lane - 1 && c.State == CarState.MovingRight || c.Lane == lane + 1 && (c.State == CarState.MovingLeft || c.State == CarState.Merging)) && Math.Abs(c.Position.Y - posY) < Math.Max(c.BodySize.Y, type.Height) + 1)
						{
							foundSpawnPos = false;
							break;
						}
					}
				} while (!foundSpawnPos && spawnAttempts < 5);

				if (foundSpawnPos) {
					float speed = (float)random.NextDouble() * 5.0f + (NumLanes - lane + 1) * 2.0f;

					Car car = new Car(type, World, speed);
					car.Position = new Vector2(posX, posY);
					car.Lane = lane;
					car.DesiredLane = lane;

					cars.Add(car);
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


		public void RenderTraffic(SpriteBatch spriteBatch, GameTime gameTime, Matrix projection, Matrix view)
		{
			foreach (Car car in cars)
			{
				car.Render(spriteBatch, gameTime, projection, view);
			}
		}

	}
}
