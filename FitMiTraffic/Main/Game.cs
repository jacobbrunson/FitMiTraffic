using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Diagnostics;
using FitMiTraffic.Main.Vehicle;
using FitMiTraffic.Main.Environment;
using FitMiTraffic.Main.Input;
using FitMiTraffic.Main.Gui;
using FitMiTraffic.Main.Utility;
using FitMiTraffic.Main.Graphics;
using FitMiTraffic.Main.Modes;

namespace FitMiTraffic.Main
{

	public class TrafficGame : Game
	{

		public static GameMode Mode;
		public static bool DEBUG;

		private GraphicsDeviceManager graphicsManager;
		private SpriteBatch spriteBatch;

		public TrafficGame()
		{
			graphicsManager = new GraphicsDeviceManager(this);
			graphicsManager.PreferredBackBufferWidth = 600;
			graphicsManager.PreferredBackBufferHeight = 800;
			graphicsManager.GraphicsProfile = GraphicsProfile.HiDef;
			graphicsManager.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
			graphicsManager.ApplyChanges();

			Content.RootDirectory = "Content";
		}

		private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
		{
			//_graphicsManager.PreferMultiSampling = true;
			//e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			Car.LoadContent(Content);
			Road.LoadContent(Content);

			MessagesUI.LoadContent(Content);
			ScoreUI.LoadContent(Content);
			GameOverUI.LoadContent(Content);

			//Effect e = Content.Load<Effect>("desaturate");
			//postProcessor = new PostProcessor(GraphicsDevice, _spriteBatch, e);

			//DebugView.LoadContent(GraphicsDevice, Content);

			Mode = new GameMode(this, GraphicsDevice, spriteBatch, Content);
		}

		protected override void Update(GameTime gameTime)
		{
			Mode.Update(gameTime);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			Mode.Render(gameTime);
			base.Draw(gameTime);
		}
	}

    /*public class TrafficGame : Game
    {

		private const int DodgePoints = 10000;

		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Vector2 cameraPosition;
		private float scale = 40f; //pixels per meter
		public static BasicEffect cameraEffect;

		private MessagesUI messagesUI;
		private ScoreUI scoreUI;
		private GameOverUI gameOverUI;

		private World world;
		private Player player;
		private EnvironmentManager environment;
		private TrafficManager trafficManager;

		private int score = 0;

		public static bool DEBUG = false;
		public static DebugView DebugView;

		Vector2 cameraRot = Vector2.Zero;

		RenderTarget2D shadowMapRenderTarget;
		public static Vector3 lightPosition = new Vector3(20, 20, 20);
		public static Vector3 lightDirection = new Vector3(-1f, -1, -1);
		Matrix lightView;

		Matrix lightProjection;

		GameState state = GameState.RUNNING;
		double stateChangeTime;
		float zoom = 1f;

		PostProcessor postProcessor;

		Ground ground;

	

        protected override void Initialize()
        {
			tainicom.Aether.Physics2D.Settings.MaxPolygonVertices = 16;

			world = new World(Vector2.Zero);
			DebugView = new DebugView(world);
			DebugView.AppendFlags(DebugViewFlags.DebugPanel | DebugViewFlags.PolygonPoints);

			InputManager.Initialize();

			environment = new EnvironmentManager(Content, world);
			trafficManager = new TrafficManager(Content, world, 1000, Road.NumLanes, Road.LaneWidth);

			messagesUI = new MessagesUI();
			scoreUI = new ScoreUI();
			gameOverUI = new GameOverUI();

			base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

			Car.LoadContent(Content);
			Road.LoadContent(Content);

			MessagesUI.LoadContent(Content);
			ScoreUI.LoadContent(Content);
			GameOverUI.LoadContent(Content);

			player = new Player(Content, CarType.SPORT, world, 20);
			player.Position = new Vector2(0, 0);
			player.DodgeCompleteCallback = DodgeCompleted;

			cameraEffect = new BasicEffect(GraphicsDevice);
			cameraEffect.TextureEnabled = false;

			shadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, 2048, 2048, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

			//ground = new Ground(Content);

			Effect e = Content.Load<Effect>("desaturate");
			postProcessor = new PostProcessor(GraphicsDevice, spriteBatch, e);

			DebugView.LoadContent(GraphicsDevice, Content);
		}


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
			HandleInput(gameTime);
			if (!player.crashed)
				world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
			else
				world.Step((float)gameTime.ElapsedGameTime.TotalSeconds/4);
			cameraPosition = new Vector2(0, player.Position.Y + 5);

			if (state == GameState.RUNNING)
			{
				if (!player.crashed)
				{
					score += (int)(player.Velocity.Y * 0.5f);
				}
			} else if (state == GameState.STARTING)
			{
				zoom = Math.Min(1, zoom + 0.05f);
				if (gameTime.TotalGameTime.TotalSeconds - stateChangeTime > 3)
				{
					state = GameState.RUNNING;
					stateChangeTime = gameTime.TotalGameTime.TotalSeconds;
					player.Velocity = new Vector2(player.Velocity.X, 20);
				}
			} else if (state == GameState.RECENTERING)
			{
				zoom = Math.Max(0, zoom-0.05f);
				if (gameTime.TotalGameTime.TotalSeconds - stateChangeTime > 1)
				{
					state = GameState.STARTING;
					stateChangeTime = gameTime.TotalGameTime.TotalSeconds;
					environment.Reset();
					trafficManager.Reset();
					player.Reset();
					score = 0;
				}
			}

			trafficManager.Update(gameTime, player, state);
			environment.Update(gameTime, player.Position.Y);
			player.Update(gameTime, InputManager.LateralMovement);



			messagesUI.Update(gameTime);
			scoreUI.Update(gameTime, score);

			lightPosition = new Vector3(lightPosition.X, player.Position.Y + 15, lightPosition.Z);

            base.Update(gameTime);
        }

		protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkOliveGreen);

			Vector3 cameraTarget = new Vector3(0 * zoom + player.Position.X * (1 - zoom), player.Position.Y + 5 * zoom, 0);
			Vector3 cameraPosition;
			if (!DEBUG)
			{
				//Vector3 cameraPosition3D = new Vector3(cameraPosition + Vector2.UnitY * -10, 0.5f);

				//cameraEffect.View = Matrix.Identity;
				cameraPosition = cameraTarget + new Vector3(0, -5 * zoom, 12 * zoom);
				cameraEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2 * 0.8f, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
			} else
			{
				//Vector3 cameraPosition3D = new Vector3(cameraPosition, 10);
				//cameraEffect.View = Matrix.CreateLookAt(cameraPosition3D, new Vector3(cameraPosition, 0), Vector3.Up);
				cameraPosition = cameraTarget + new Vector3(0, 0, 10);
				cameraEffect.Projection = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width / scale, GraphicsDevice.Viewport.Height / scale, -1000f, 1000f);
			}
			
			cameraPosition = Vector3.Transform(cameraPosition - cameraTarget, Matrix.CreateFromYawPitchRoll(cameraRot.X, cameraRot.Y, 0)) + cameraTarget;
			cameraEffect.View = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
			//cameraEffect.Alpha = 1f;


			lightProjection = Matrix.CreateOrthographic(25, 50, 10, 40);
			lightView = Matrix.CreateLookAt(lightPosition,
						lightPosition + lightDirection,
						Vector3.Up);
			Matrix lightViewProjection = lightView * lightProjection;
			GraphicsDevice.SetRenderTarget(shadowMapRenderTarget);
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
			//GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			environment.Render(GraphicsDevice, spriteBatch, gameTime, cameraEffect.View, cameraEffect.Projection, lightViewProjection, shadowMapRenderTarget, "ShadowMap");
			player.Render(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View, lightViewProjection, shadowMapRenderTarget, "ShadowMap");
			trafficManager.RenderTraffic(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View, lightViewProjection, shadowMapRenderTarget, "ShadowMap");
			graphics.GraphicsDevice.SetRenderTarget(null);

			

			postProcessor.Begin();

			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			
			//ground.Render(GraphicsDevice, cameraEffect.View, cameraEffect.Projection);
			//GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			//GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
			environment.Render(GraphicsDevice, spriteBatch, gameTime, cameraEffect.View, cameraEffect.Projection, lightViewProjection, shadowMapRenderTarget, "ShadowedScene");
			player.Render(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View, lightViewProjection, shadowMapRenderTarget, "ShadowedCar");
			trafficManager.RenderTraffic(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View, lightViewProjection, shadowMapRenderTarget, "ShadowedScene");

			postProcessor.End(player.crashed);

			GraphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin(0, BlendState.Opaque, SamplerState.AnisotropicClamp);
			//spriteBatch.Draw(shadowMapRenderTarget, new Rectangle(0, 0, 600, 800), Color.White);
			spriteBatch.End();

	
		

			spriteBatch.Begin();


			messagesUI.Render(spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			scoreUI.Render(spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

			if (player.crashed && state != GameState.RECENTERING)
			{
				gameOverUI.Render(spriteBatch, gameTime, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			}


			spriteBatch.End();

            if (DEBUG)
            {
				DebugView.RenderDebugData(cameraEffect.Projection, cameraEffect.View, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, 0.8f);
				
            }

            base.Draw(gameTime);
        }
    }*/
}
