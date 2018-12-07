using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NewTrafficRacer.Environment;
using NewTrafficRacer.Graphics;
using NewTrafficRacer.Gui;
using NewTrafficRacer.Input;
using NewTrafficRacer.Vehicle;
using tainicom.Aether.Physics2D.Dynamics;

namespace NewTrafficRacer.Modes
{
	class MenuMode : Mode
	{
		//Parameters
		private Vector3 initialLightPosition = new Vector3(10, -5, 20);
		private Vector3 lightDirection = new Vector3(-1, 1, -1);
		private const int groundWidth = 16;
		private Vector4 ambientColor = new Vector4(1, 0.8f, 0.8f, 1);
		private const float ambientIntensity = 0.4f;
		private const float diffuseIntensity = 0.8f;
        private const int shadowMapRes = 8;

		//Things
		private World world;
		private Road road;
		private Car car;

		//Graphics
		private Camera camera;
		private Lighting lighting;
		private Effect effect;
		private PostProcessor postProcessor;
		private Texture2D sky;

		//GUI
		private TitleUI titleUI;
        private FPSUI fpsUI;

		public MenuMode(TrafficGame game, GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content) : base(game, graphics, spriteBatch, content)
		{
			world = new World(Vector2.Zero);
			road = new Road(world, groundWidth, groundWidth / 2, 1f);
			road.CullBack = false;
			road.Reset();
			//roadSegments = new LinkedList<RoadSegment>();
			for (int i = 0; i < 15; i++)
			{
				//roadSegments.AddLast(new RoadSegment(content, world, -i * Road.Size + 2.5f, groundWidth, groundWidth/2));
			}

			car = new Car(content, CarType.SPORT, world, 0);
			car.Position = new Vector2(Road.LaneWidth/2, 50);
			car.model.Color = new Color(229, 189, 15, 255);

			camera = new Camera(graphics.Viewport.Width, graphics.Viewport.Height);
			camera.Mode = CameraMode.FIXED;
			camera.Offset = new Vector2(2, 3);

			//camera.Mode = CameraMode.FLAT;
			//camera.Scale = 5;
			//camera.Target = new Vector2(4, -60);

			lighting = new Lighting(graphics, shadowMapRes);
			lighting.Position = initialLightPosition;
			lighting.Direction = lightDirection;

            Console.WriteLine("tryna load effect, depthoffield");

            effect = content.Load<Effect>("effect");

			postProcessor = new PostProcessor(graphics, spriteBatch, content.Load<Effect>("depthoffield"));

			TitleUI.LoadContent(content);
			titleUI = new TitleUI();

            FPSUI.LoadContent(content);
            fpsUI = new FPSUI();
            Console.WriteLine("tryna load sky");

            sky = content.Load<Texture2D>("sky");
		}
		public override void Update(GameTime gameTime)
		{
			InputManager.Update();

			if (InputManager.Quit)
			{
				game.Exit();
			}

            if (InputManager.Enter)
            {
                //game.Play(); //COMMENTED FOR COMPILE
            }

            if (InputManager.ToggleDebug)
            {
                TrafficGame.DEBUG = !TrafficGame.DEBUG;
            }


                car.Position = car.Position + Vector2.UnitY * 5 * (float) gameTime.ElapsedGameTime.TotalSeconds;

			road.Update(car.Position.Y);
			lighting.Position = initialLightPosition + new Vector3(car.Position, 0);
			camera.Target = car.Position + Vector2.UnitY * 1.5f;
			camera.Revolution += InputManager.MoveCameraAmount / 30;

			titleUI.Update(gameTime);
            fpsUI.Update(gameTime);
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
			//effect.LightViewProjection = lighting.View * lighting.Projection; //COMMENTED FOR COMPILE
			effect.Parameters["xLightPos"].SetValue(lighting.Position);
			effect.Parameters["DiffuseLightDirection"].SetValue(lighting.Direction);
			effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);
			effect.Parameters["AmbientColor"].SetValue(ambientColor);
			effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

			//Render shadow map
			graphics.SetRenderTarget(lighting.ShadowMap);
			graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
			effect.CurrentTechnique = effect.Techniques["ShadowMap"];

			road.Render(gameTime, graphics, effect);
			car.Render(gameTime, effect);


            //Render scene
            graphics.SetRenderTarget(null);
			//postProcessor.Begin();

			graphics.Clear(Color.Orange);

			spriteBatch.Begin();
			spriteBatch.Draw(sky, Vector2.Zero, Color.White);
			spriteBatch.End();
			graphics.BlendState = BlendState.Opaque;
			graphics.DepthStencilState = DepthStencilState.Default;

            //effect.View = camera.View; //COMMENTED FOR COMPILE
            //effect.Projection = camera.Projection; //COMMENTED FOR COMPILE
            //effect.ShadowMap = lighting.ShadowMap; //COMMENTED FOR COMPILE

            effect.CurrentTechnique = effect.Techniques["ShadowedScene"];
			road.Render(gameTime, graphics, effect);
			effect.CurrentTechnique = effect.Techniques["ShadowedCar"];
			car.Render(gameTime, effect);

			//postProcessor.End();

			spriteBatch.Begin();
			titleUI.Render(spriteBatch, gameTime, graphics.Viewport.Width, graphics.Viewport.Height);
            if (TrafficGame.DEBUG)
            {
                fpsUI.Render(spriteBatch, 600, 800);
            }
			spriteBatch.End();
        }
	}
}
