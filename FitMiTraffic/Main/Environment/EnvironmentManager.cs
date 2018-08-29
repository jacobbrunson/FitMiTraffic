using FitMiTraffic.Main.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Environment
{
	class EnvironmentManager
	{
		private Road road = new Road();
		private List<RenderedModel> models = new List<RenderedModel>();

		private ContentManager content;

		public EnvironmentManager(ContentManager content)
		{
			this.content = content;

			RenderedModel model;

			model = new SpeedLimit(content) { Position = new Vector3(-6, 10, 0) };
			models.Add(model);

			model = new SpeedLimit(content) { Position = new Vector3(-6, 100, 0) };
			models.Add(model);

			model = new SpeedLimit(content) { Position = new Vector3(-6, 400, 0) };
			models.Add(model);

			model = new BigSign(content) { Position = new Vector3(0, 100, 0) };
			models.Add(model);

			model = new ExitSign(content) { Position = new Vector3(5.5f, 50, 0) };
			models.Add(model);
		}

		public void Add(RenderedModel model)
		{
			models.Add(model);
		}

		public void Update(GameTime gameTime, float playerY)
		{
			road.Update(playerY);
		}

		public void Render(SpriteBatch spriteBatch, GameTime gameTime, Matrix view, Matrix projection)
		{
			road.Render(spriteBatch);
			foreach (RenderedModel model in models)
			{
				model.Render(gameTime, view, projection);
			}
		}
	}
}
