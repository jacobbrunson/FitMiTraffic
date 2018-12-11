using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Diagnostics;
using NewTrafficRacer.Modes;
using NewTrafficRacer.Environment;
using NewTrafficRacer.Utility;

namespace NewTrafficRacer.Vehicle
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

        private Random random;

        public TrafficManager(ContentManager content, World world, int numLanes, float laneWidth)
        {
            World = world;
            Content = content;
            NumLanes = numLanes;
            LaneWidth = laneWidth;

            if (TrafficGame.Difficulty > 0.3f)
            {
                Interval = (int)TrafficGame.Difficulty.Map(0.3f, 1, 10000, 1000);
                LastSpawnMillis = -Interval;
            }
            else
            {
                Interval = int.MaxValue;
                LastSpawnMillis = int.MaxValue;
            }

            random = new Random();
        }

        public void RenderDebug(DebugView debugView, Matrix view, Matrix projection)
        {
            foreach (Car car in cars)
            {
                foreach (CastedRay ray in car.Rays)
                {
                    debugView.BeginCustomDraw(projection, view);

                    if (ray.Hit)
                    {
                        debugView.DrawPoint(ray.P1, .25f, new Color(0.9f, 0.4f, 0.4f));
                        debugView.DrawSegment(ray.P2, ray.P1, new Color(0.8f, 0.4f, 0.4f));
                    }
                    else
                    {
                        debugView.DrawPoint(ray.P1, .25f, new Color(0.4f, 0.9f, 0.4f));
                        debugView.DrawSegment(ray.P2, ray.P1, new Color(0.8f, 0.8f, 0.8f));
                    }
                    debugView.EndCustomDraw();
                }

                car.Rays.Clear();
            }
        }

        public void Reset()
        {
            foreach (Car car in cars)
            {
                World.Remove(car.Body);
            }

            cars.Clear();
        }

        public void Update(GameTime gameTime, Player player, GameState state)
        {
            float playerY = player.Position.Y;


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

                    if (player.Velocity.Y < 10 || state == GameState.STARTING)
                    {
                        posY = playerY - (float)(random.NextDouble()) * 20 - 10;
                    }
                    else
                    {
                        posY = playerY + (float)(random.NextDouble()) * 20 * player.Velocity.Y.Map(10, 30, 0, 1) + 30;
                    }

                    foundSpawnPos = true;
                    foreach (Car c in cars)
                    {
                        if ((c.Lane == lane || c.Lane == lane - 1 && c.State == CarState.MovingRight || c.Lane == lane + 1 && (c.State == CarState.MovingLeft || c.State == CarState.Merging)) && Math.Abs(c.Position.Y - posY) < Math.Max(c.BodySize.Y, type.Length) + 1)
                        {
                            foundSpawnPos = false;
                            break;
                        }
                    }
                } while (!foundSpawnPos && spawnAttempts < 5);

                if (foundSpawnPos)
                {
                    float speed = player.Velocity.Y + (NumLanes - lane + 1) * 1.0f + (float)random.NextDouble().Map(0, 1, -10, 0);
                    speed *= (1 - Math.Min(0.5f, TrafficGame.Difficulty));

                    Car car = new Car(Content, type, World, speed);
                    car.Position = new Vector2(posX, posY);
                    car.Lane = lane;
                    car.DesiredLane = lane;

                    cars.Add(car);
                }
            }

            //Despawn cars we left in the dust
            for (int i = cars.Count - 1; i >= 0; i--)
            {
                if (cars[i].Body.Position.Y < playerY - 30 || cars[i].Body.Position.Y > playerY + 50)
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


        public void RenderTraffic(GameTime gameTime, Effect effect)
        {
            foreach (Car car in cars)
            {
                car.Render(gameTime, effect);
            }
        }
    }
}