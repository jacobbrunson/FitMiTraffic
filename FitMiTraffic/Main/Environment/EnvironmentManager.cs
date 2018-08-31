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
		private Road road;
		private Ground ground;
		private List<RenderedModel> models = new List<RenderedModel>();

		private ContentManager content;

		public EnvironmentManager(ContentManager content)
		{
			this.content = content;
			Road.LoadContent(content);
			road = new Road();

			RenderedModel model;

			ground = new Ground(content);
			models.Add(ground);

			model = new SpeedLimit(content) { Position = new Vector3(-6, 10, 0) };
			//models.Add(model);

			model = new SpeedLimit(content) { Position = new Vector3(-6, 200, 0) };
			//models.Add(model);

			model = new SpeedLimit(content) { Position = new Vector3(-6, 400, 0) };
			//models.Add(model);

			model = new BigSign(content) { Position = new Vector3(0, 10, 0) };
			models.Add(model);

			model = new ExitSign(content) { Position = new Vector3(5.5f, 50, 0) };
			//models.Add(model);
		}

		public void Add(RenderedModel model)
		{
			models.Add(model);
		}

		public void Update(GameTime gameTime, float playerY)
		{
			road.Update(playerY);
			ground.Position = new Vector3(0, playerY, 0);
		}

		public void Render(SpriteBatch spriteBatch, GameTime gameTime, Matrix view, Matrix projection, Matrix lightViewProjection, Texture2D shadowMap, string technique)
		{
			road.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
			foreach (RenderedModel model in models)
			{
				model.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
			}
		}

		public void RenderShadowMap(GraphicsDevice graphics, Matrix lightViewProjection)
		{
			foreach (RenderedModel model in models)
			{
				model.RenderShadowMap(graphics, lightViewProjection);
			}
		}
	}
}
