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

namespace FitMiTraffic.Main
{
    public class TrafficGame : Game
    {

		private const int DodgePoints = 1000;

		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Vector2 cameraPosition;
		private float scale = 40f; //pixels per meter
		public static BasicEffect cameraEffect;

		private MessagesUI messagesUI;
		private ScoreUI scoreUI;

		private World world;
		private Player player;
		private EnvironmentManager environment;
		private TrafficManager trafficManager;

		private int score = 0;

		public static bool DEBUG = false;
		public static DebugView DebugView;

		Vector2 cameraRot = Vector2.Zero;

		RenderTarget2D shadowMapRenderTarget;
		public static Vector3 lightPosition = new Vector3(20, 0, 20);
		public static Vector3 lightDirection = new Vector3(-1f, -1, -1);
		Matrix lightView;

		Matrix lightProjection;

		GameState state = GameState.STARTING;
		double startingTime;

		public enum GameState
		{
			STARTING, RUNNING, RECENTERING
		}

		private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
		{
			//graphics.PreferMultiSampling = true;
			//e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
		}
		public TrafficGame()
        {
            graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 600;
			graphics.PreferredBackBufferHeight = 800;
			graphics.GraphicsProfile = GraphicsProfile.HiDef;
			graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
			graphics.ApplyChanges();

			Content.RootDirectory = "Content";
        }

		private void DodgeCompleted()
		{
			messagesUI.WriteMessage("NICE DODGE!");
			scoreUI.ShowPoints(DodgePoints);
			score += DodgePoints;
		}

		private void HandleInput(GameTime gameTime)
		{
			InputManager.Update();

			if (InputManager.Escape)
			{
				Exit();
			}

			if (InputManager.ToggleDebug)
			{
				DEBUG = !DEBUG;
				cameraRot = Vector2.Zero;
			}

			if (InputManager.ZoomOut)
			{
				scale /= 1.5f;
			}

			if (InputManager.ZoomIn)
			{
				scale *= 1.5f;
			}

			if (InputManager.Restart)
			{
				player.Recenter(gameTime);
				state = GameState.RECENTERING;
			}

			cameraRot += InputManager.MoveCameraAmount / 30;
		}

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

			base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

			Car.LoadContent(Content);
			Road.LoadContent(Content);

			MessagesUI.LoadContent(Content);
			ScoreUI.LoadContent(Content);

			player = new Player(Content, CarType.TEST1, world, 5);
			player.Position = new Vector2(0, -5);
			player.DodgeCompleteCallback = DodgeCompleted;

			cameraEffect = new BasicEffect(GraphicsDevice);
			cameraEffect.TextureEnabled = true;

			shadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, 2048, 2048, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);


			DebugView.LoadContent(GraphicsDevice, Content);
		}


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
			HandleInput(gameTime);
			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
			cameraPosition = new Vector2(0, player.Position.Y + 5);

			if (state == GameState.RUNNING)
			{
				if (!player.crashed)
				{
					score += (int)(player.Velocity.Y * 1);
				}
			} else if (state == GameState.STARTING)
			{
				if (gameTime.TotalGameTime.TotalSeconds - startingTime > 3)
				{
					state = GameState.RUNNING;
					Console.WriteLine("yeaaa");
					player.Velocity = new Vector2(player.Velocity.X, 20);
				}
			} else if (state == GameState.RECENTERING)
			{
				if (!player.IsRecentering())
				{
					state = GameState.STARTING;
					startingTime = gameTime.TotalGameTime.TotalSeconds;
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

			Vector3 cameraTarget = new Vector3(0, player.Position.Y + 5, 0);
			Vector3 cameraPosition;
			if (!DEBUG)
			{
				//Vector3 cameraPosition3D = new Vector3(cameraPosition + Vector2.UnitY * -10, 0.5f);

				//cameraEffect.View = Matrix.Identity;
				cameraPosition = cameraTarget + new Vector3(0, -5, 12);
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


			lightProjection = Matrix.CreateOrthographic(25, 50, 0, 50);
			lightView = Matrix.CreateLookAt(lightPosition,
						lightPosition + lightDirection,
						Vector3.Up);
			Matrix lightViewProjection = lightView * lightProjection;
			GraphicsDevice.SetRenderTarget(shadowMapRenderTarget);
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
			//GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			environment.Render(spriteBatch, gameTime, cameraEffect.View, cameraEffect.Projection, lightViewProjection, shadowMapRenderTarget, "ShadowMap");
			player.Render(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View, lightViewProjection, shadowMapRenderTarget, "ShadowMap");
			trafficManager.RenderTraffic(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View, lightViewProjection, shadowMapRenderTarget, "ShadowMap");
			graphics.GraphicsDevice.SetRenderTarget(null);

			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
			//cameraEffect.View = lightView;
			//cameraEffect.Projection = lightProjection;

			environment.Render(spriteBatch, gameTime, cameraEffect.View, cameraEffect.Projection, lightViewProjection, shadowMapRenderTarget, "ShadowedScene");
			player.Render(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View, lightViewProjection, shadowMapRenderTarget, "ShadowedScene");
			trafficManager.RenderTraffic(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View, lightViewProjection, shadowMapRenderTarget, "ShadowedScene");


			GraphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin(0, BlendState.Opaque, SamplerState.AnisotropicClamp);
			//spriteBatch.Draw(shadowMapRenderTarget, new Rectangle(0, 0, 600, 800), Color.White);
			spriteBatch.End();

		

			spriteBatch.Begin();


			messagesUI.Render(spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			scoreUI.Render(spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);


			spriteBatch.End();

            if (DEBUG)
            {
				DebugView.RenderDebugData(cameraEffect.Projection, cameraEffect.View, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, 0.8f);
				
            }

            base.Draw(gameTime);
        }
    }
}
