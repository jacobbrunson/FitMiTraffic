using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FitMiTraffic.Main.Environment;
using tainicom.Aether.Physics2D.Dynamics;
using FitMiTraffic.Main.Graphics;
using FitMiTraffic.Main.Vehicle;

namespace FitMiTraffic.Main.Modes
{
	class MenuMode : Mode
	{
		//Parameters
		private Vector3 initialLightPosition = new Vector3(20, 20, 20);
		private Vector3 lightDirection = new Vector3(-1, -1, -1);

		//Things
		private World world;
		private Road road;
		private Car car;

		//Graphics
		private Camera camera;
		private Lighting lighting;
		private BaseEffect effect;

		public MenuMode(TrafficGame game, GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content) : base(game, graphics, spriteBatch, content)
		{
			world = new World(Vector2.Zero);
			road = new Road(world);

			car = new Car(content, CarType.SPORT, world, 0);
			//car.model.Color = Color.Red;

			camera = new Camera(graphics.Viewport.Width, graphics.Viewport.Height);
			camera.Mode = CameraMode.FIXED;
			camera.Target = new Vector2(5, 5);

			lighting = new Lighting(graphics);
			lighting.Position = initialLightPosition;
			lighting.Direction = lightDirection;

			effect = new BaseEffect(content.Load<Effect>("effect"));
		}
		public override void Update(GameTime gameTime)
		{

		}
		public override void Render(GameTime gameTime)
		{
			graphics.Clear(Color.Black);

			//Update lights and camera
			camera.Update();
			lighting.Update();

			//Update graphics state
			graphics.BlendState = BlendState.Opaque;
			graphics.DepthStencilState = DepthStencilState.Default;

			//Set lighting parameters
			effect.LightViewProjection = lighting.View * lighting.Projection;
			effect.Parameters["xLightPos"].SetValue(lighting.Position);
			effect.Parameters["DiffuseLightDirection"].SetValue(lighting.Direction);

			//Render shadow map
			graphics.SetRenderTarget(lighting.ShadowMap);
			graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
			effect.Technique = effect.Techniques["ShadowMap"];

			road.Render(gameTime, graphics, effect);
			car.Render(gameTime, effect);


			//Render scene
			graphics.SetRenderTarget(null);

			//postProcessor.Begin();

			effect.View = camera.View;
			effect.Projection = camera.Projection;
			effect.ShadowMap = lighting.ShadowMap;

			effect.Technique = effect.Techniques["ShadowedScene"];
			road.Render(gameTime, graphics, effect);
			effect.Technique = effect.Techniques["ShadowedCar"];
			car.Render(gameTime, effect);

			//postProcessor.End(player.crashed);
		}
	}
}
