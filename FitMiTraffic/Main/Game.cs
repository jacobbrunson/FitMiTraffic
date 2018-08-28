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
		private Road road;
		private TrafficManager trafficManager;

		private int score = 0;

		public static bool DEBUG = true;
		public static DebugView DebugView;

		public TrafficGame()
        {
            graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 600;
			graphics.PreferredBackBufferHeight = 800;

            Content.RootDirectory = "Content";
        }

		private void DodgeCompleted()
		{
			messagesUI.WriteMessage("NICE DODGE!");
			scoreUI.ShowPoints(DodgePoints);
			score += DodgePoints;
		}

		private void HandleInput()
		{
			InputManager.Update();

			if (InputManager.Escape)
			{
				Exit();
			}

			if (InputManager.ToggleDebug)
			{
				DEBUG = !DEBUG;
			}

			if (InputManager.ZoomOut)
			{
				scale /= 1.5f;
			}

			if (InputManager.ZoomIn)
			{
				scale *= 1.5f;
			}
		}

        protected override void Initialize()
        {
			tainicom.Aether.Physics2D.Settings.MaxPolygonVertices = 16;

			world = new World(Vector2.Zero);
			DebugView = new DebugView(world);
			DebugView.AppendFlags(DebugViewFlags.DebugPanel | DebugViewFlags.PolygonPoints);

			InputManager.Initialize();

			road = new Road();
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

			player = new Player(CarType.TEST, world, 20);
			player.Position = new Vector2(0, -10);
			player.DodgeCompleteCallback = DodgeCompleted;

			cameraEffect = new BasicEffect(GraphicsDevice);
			cameraEffect.TextureEnabled = true;

			DebugView.LoadContent(GraphicsDevice, Content);
		}


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
			HandleInput();

			trafficManager.Update(gameTime, player.Position.Y);

			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
			
			cameraPosition = new Vector2(0, player.Position.Y + 5);
			road.Update(player.Position.Y);
			player.Update(gameTime, InputManager.LateralMovement);

			if (!player.crashed)
			{
				score += (int)(player.Velocity.Y * 1);
			}

			messagesUI.Update(gameTime);
			scoreUI.Update(gameTime, score);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkOliveGreen);

			if (!DEBUG)
			{
				Vector3 cameraPosition3D = new Vector3(cameraPosition + Vector2.UnitY * -5, 10);
				cameraEffect.View = Matrix.CreateLookAt(cameraPosition3D, new Vector3(cameraPosition, 0), Vector3.Up);
				cameraEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);
			} else
			{
				Vector3 cameraPosition3D = new Vector3(cameraPosition, 10);
				cameraEffect.View = Matrix.CreateLookAt(cameraPosition3D, new Vector3(cameraPosition, 0), Vector3.Up);
				cameraEffect.Projection = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width / scale, GraphicsDevice.Viewport.Height / scale, -1000f, 1000f);
			}
			cameraEffect.Alpha = 1f;

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, RasterizerState.CullNone, cameraEffect);

			road.Render(spriteBatch);
			spriteBatch.End();


			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			player.Render(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View);
			trafficManager.RenderTraffic(spriteBatch, gameTime, cameraEffect.Projection, cameraEffect.View);

            

			if (DEBUG)
			{
				DebugView.RenderDebugData(cameraEffect.Projection, cameraEffect.View);
			}

			spriteBatch.Begin();


			messagesUI.Render(spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			scoreUI.Render(spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);


			spriteBatch.End();

			base.Draw(gameTime);
        }
    }
}
