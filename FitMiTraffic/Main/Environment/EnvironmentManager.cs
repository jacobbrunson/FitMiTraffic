using FitMiTraffic.Main.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace FitMiTraffic.Main.Environment
{
	class EnvironmentManager
	{
		private Road road;
		//private Ground ground;
		private List<RenderedModel> models = new List<RenderedModel>();

		private ContentManager content;

		public EnvironmentManager(ContentManager content, World world)
		{
			this.content = content;
			Road.LoadContent(content);
			road = new Road(world);

			RenderedModel model;

			//ground = new Ground(content);
			//models.Add(ground);

			model = new SpeedLimit(content) { Position = new Vector3(-6, 10, 0) };
			//models.Add(model);

			model = new SpeedLimit(content) { Position = new Vector3(-6, 200, 0) };
			//models.Add(model);

			model = new SpeedLimit(content) { Position = new Vector3(-6, 400, 0) };
			//models.Add(model);

			model = new BigSign(content) { Position = new Vector3(0, 100, 0) };
			models.Add(model);

			model = new ExitSign(content) { Position = new Vector3(5.5f, 50, 0) };
			//models.Add(model);
		}

		public void Reset()
		{
			road.Reset();
		}

		public void Add(RenderedModel model)
		{
			models.Add(model);
		}

		public void Update(GameTime gameTime, float playerY)
		{
			road.Update(playerY);
			//ground.Position = new Vector2(0, playerY);
		}

		public void Render(GameTime gameTime, GraphicsDevice graphics, BaseEffect effect)
		{
			road.Render(gameTime, graphics, effect);
			foreach (RenderedModel model in models)
			{
				model.Render(gameTime, effect);
			}
		}
	}
}
