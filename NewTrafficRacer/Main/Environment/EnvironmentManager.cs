using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NewTrafficRacer.Graphics;
using NewTrafficRacer.Utility;
using NewTrafficRacer.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace NewTrafficRacer.Environment
{
	class EnvironmentManager
	{
		public Road road;
		//Ground ground;
		List<RenderedModel> models = new List<RenderedModel>();

        float lastBigSignSpawn = 0;
        float lastCoinSpawn = 0;

		ContentManager content;

        Dictionary<Body, Coin> coins = new Dictionary<Body, Coin>();

        Queue<Body> coinDeleteQueue = new Queue<Body>();

        World world;

		public EnvironmentManager(ContentManager content, World world)
		{
			this.content = content;
            this.world = world;
			Road.LoadContent(content);
			road = new Road(world, 24);            
		}

        public void DestroyCoin(Body b)
        {
            coinDeleteQueue.Enqueue(b);
        }

		public void Reset()
		{
			road.Reset();
		}

		public void Add(RenderedModel model)
		{
			models.Add(model);
		}

		public void Update(GameTime gameTime, Player player)
		{
			road.Update(gameTime, player.Position.Y);

            while (coinDeleteQueue.Count > 0)
            {
                Body b = coinDeleteQueue.Dequeue();
                world.Remove(b);
                coins.Remove(b);
            }

            foreach (KeyValuePair<Body, Coin> pair in coins)
            {
                if (pair.Value.Position.Y < player.Position.Y - 10)
                {
                    DestroyCoin(pair.Key);
                }
                pair.Value.Update(gameTime);
            }

            for (int i = models.Count-1; i >= 0; i--)
            {
                RenderedModel model = models[i];
                if (model.Position.Y + 20 < player.Position.Y)
                {
                    models.RemoveAt(i);
                }
            }

            if (player.Position.Y > lastBigSignSpawn + 200)
            {
                lastBigSignSpawn = player.Position.Y + 30;
                BigSign model = BigSign.Instantiate(content);
                model.Position = new Vector3(0, lastBigSignSpawn, 0);
                models.Add(model);
            }

            if (TrafficGame.Difficulty >= 0.2f && player.Position.Y > lastCoinSpawn + 10)
            {
                Random r = new Random();
                lastCoinSpawn = player.Position.Y + r.Next(30, 40);
                Vector2 pos = new Vector2(r.NextDouble().Map(0, 1, -Road.Size/2, Road.Size/2), player.Position.Y + lastCoinSpawn);
                var coin = new Coin(content, world, pos);
                coins.Add(coin.Body, coin);
            }
			//ground.Position = new Vector2(0, playerY);
		}

		public void Render(GameTime gameTime, GraphicsDevice graphics, Effect effect)
		{
			road.Render(gameTime, graphics, effect);
			foreach (RenderedModel model in models)
			{
				model.Render(gameTime, effect);
			}
            foreach (KeyValuePair<Body, Coin> pair in coins)
            {
                pair.Value.Render(gameTime, effect);
            }
        }
	}
}
