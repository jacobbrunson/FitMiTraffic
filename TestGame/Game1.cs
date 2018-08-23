using FitMi_Research_Puck;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Diagnostics;

namespace TestGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        Texture2D car_texture;
		Texture2D blue_car;
		Texture2D road_texture;

		Player player;

		List<Car> cars = new List<Car>();

		Road Road;

		Vector2 cameraPosition;
		float scale = 50.0f; // 50 pixels per meter

		public static BasicEffect cameraEffect; //TODO: probably shouldn't be public nor static

        World world;
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

		HIDPuckDongle puck;

		TrafficManager Traffic;

		public static bool DEBUG = true;
		private KeyboardState prevKeyState;

		public static DebugView DebugView;

		public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 600;
			graphics.PreferredBackBufferHeight = 800;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
			tainicom.Aether.Physics2D.Settings.MaxPolygonVertices = 16;

			// TODO: Add your initialization logic here
			world = new World(Vector2.Zero);
			//carBody.LinearVelocity = Vector2.UnitY * 5.0f;



			puck = new HIDPuckDongle();
			puck.Open();
			puck.SendCommand(0, HidPuckCommands.SENDVEL, 0x00, 0x01);
			puck.SendCommand(1, HidPuckCommands.SENDVEL, 0x00, 0x01);
			base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

			car_texture = Content.Load<Texture2D>(CarType.MERCEDES.TextureName);
			road_texture = Content.Load<Texture2D>("road");

			DebugShape.TextureRectangle = Content.Load<Texture2D>("outline_square");
			DebugShape.TextureCircle = Content.Load<Texture2D>("outline_circle");

			Road = new Road(road_texture);

			Traffic = new TrafficManager(Content, world, 1000, Road.NumLanes, Road.LaneWidth);


			player = new Player(CarType.MERCEDES, world, car_texture);
			player.Velocity = new Vector2(0, 15.0f);
			player.Body.LinearDamping = 0.0f;

			cameraEffect = new BasicEffect(GraphicsDevice);
			cameraEffect.TextureEnabled = true;

			DebugView = new DebugView(world);
			DebugView.LoadContent(GraphicsDevice, Content);
			DebugView.AppendFlags(DebugViewFlags.DebugPanel | DebugViewFlags.PolygonPoints);
        }


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            var kstate = Keyboard.GetState();
			puck.CheckForNewPuckData();

			if (prevKeyState.IsKeyUp(Keys.Z) && kstate.IsKeyDown(Keys.Z))
			{
				DEBUG = !DEBUG;
			}

			Traffic.Update(gameTime, player.Position.Y);

			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

			cameraPosition = new Vector2(0, player.Position.Y + 5);
			Road.Update(player.Position.Y);
			player.Update(gameTime, kstate, puck.PuckPack0.Gyrometer[0]);

			prevKeyState = kstate;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkOliveGreen);

			Vector3 cameraPosition3D = new Vector3(cameraPosition, 0);
			cameraEffect.View = Matrix.CreateLookAt(new Vector3(cameraPosition, 0), cameraPosition3D + Vector3.Forward, Vector3.Up);
			cameraEffect.Projection = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width / scale, GraphicsDevice.Viewport.Height / scale, 0, -1.0f);
			cameraEffect.Alpha = 1f;

			spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, cameraEffect);

			Road.Render(spriteBatch);

			if (!DEBUG)
			{
				player.Render(spriteBatch);
				Traffic.RenderTraffic(spriteBatch);
			}

            spriteBatch.End();

			if (DEBUG)
			{
				cameraEffect.Alpha = 0.25f;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, cameraEffect);
				player.Render(spriteBatch);
				Traffic.RenderTraffic(spriteBatch);
				spriteBatch.End();

				DebugView.RenderDebugData(cameraEffect.Projection, cameraEffect.View);
			}

			base.Draw(gameTime);
        }
    }
}
