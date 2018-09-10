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
using FitMiTraffic.Main.Input;
using FitMiTraffic.Main.Gui;

namespace FitMiTraffic.Main.Modes
{
	class MenuMode : Mode
	{
		//Parameters
		private Vector3 initialLightPosition = new Vector3(10, -5, 20);
		private Vector3 lightDirection = new Vector3(-1, 1, -1);
		private const int groundWidth = 16;

		//Things
		private World world;
		private Road road;
		private Car car;

		//Graphics
		private Camera camera;
		private Lighting lighting;
		private BaseEffect effect;
		private PostProcessor postProcessor;
		private Texture2D sky;

		//GUI
		private TitleUI titleUI;

		public MenuMode(TrafficGame game, GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content) : base(game, graphics, spriteBatch, content)
		{
			world = new World(Vector2.Zero);
			road = new Road(world, groundWidth, groundWidth / 2, 5);
			road.CullBack = false;
			road.Reset();
			//roadSegments = new LinkedList<RoadSegment>();
			for (int i = 0; i < 15; i++)
			{
				//roadSegments.AddLast(new RoadSegment(content, world, -i * Road.Size + 2.5f, groundWidth, groundWidth/2));
			}

			car = new Car(content, CarType.SPORT, world, 0);
			car.Position = new Vector2(Road.LaneWidth/2, 0);
			car.model.Color = new Color(229, 189, 15, 255);

			camera = new Camera(graphics.Viewport.Width, graphics.Viewport.Height);
			camera.Mode = CameraMode.FIXED;
			camera.Offset = new Vector2(2, 3);

			//camera.Mode = CameraMode.FLAT;
			//camera.Scale = 5;
			//camera.Target = new Vector2(4, -60);

			lighting = new Lighting(graphics, 4096);//, 35, 100);
			lighting.Position = initialLightPosition;
			lighting.Direction = lightDirection;

			effect = new BaseEffect(content.Load<Effect>("effect"));

			postProcessor = new PostProcessor(graphics, spriteBatch, content.Load<Effect>("depthoffield"));

			TitleUI.LoadContent(content);
			titleUI = new TitleUI();

			sky = content.Load<Texture2D>("sky");
		}
		public override void Update(GameTime gameTime)
		{
			InputManager.Update();

			if (InputManager.Escape)
			{
				game.Exit();
			}


			car.Position = car.Position + Vector2.UnitY * 5 * (float) gameTime.ElapsedGameTime.TotalSeconds;

			road.Update(car.Position.Y);
			lighting.Position = initialLightPosition + new Vector3(car.Position, 0);
			camera.Target = car.Position + Vector2.UnitY * 1.5f;
			camera.Revolution += InputManager.MoveCameraAmount / 30;

			titleUI.Update(gameTime);
		}
		public override void Render(GameTime gameTime)
		{	
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
			postProcessor.Begin();

			graphics.Clear(Color.Orange);

			spriteBatch.Begin();
			spriteBatch.Draw(sky, Vector2.Zero, Color.White);
			spriteBatch.End();
			graphics.BlendState = BlendState.Opaque;
			graphics.DepthStencilState = DepthStencilState.Default;

			effect.View = camera.View;
			effect.Projection = camera.Projection;
			effect.ShadowMap = lighting.ShadowMap;

			effect.Technique = effect.Techniques["ShadowedScene"];
			road.Render(gameTime, graphics, effect);
			effect.Technique = effect.Techniques["ShadowedCar"];
			car.Render(gameTime, effect);

			postProcessor.End();

			spriteBatch.Begin();
			titleUI.Render(spriteBatch, gameTime, graphics.Viewport.Width, graphics.Viewport.Height);
			spriteBatch.End();
		}
	}
}
