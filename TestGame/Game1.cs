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
	class Message
	{
		public String Text;
		public double Expiration;
		public Vector2 Offset;

		public Message(String text)
		{
			Text = text;
			Expiration = -1;
		}
	}
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

		TrafficManager TrafficManager;

		public static bool DEBUG = true;
		private KeyboardState prevKeyState;

		public static DebugView DebugView;

		private static Queue<Message> messages = new Queue<Message>();
		private static Queue<Message> scoreMessages = new Queue<Message>();

		private SpriteFont Font;

		static int score = 0;

		public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 600;
			graphics.PreferredBackBufferHeight = 800;

            Content.RootDirectory = "Content";
        }

		private static void AddScore(int points)
		{
			score += points;

			Random r = new Random();

			Message m = new Message(String.Format("+{0:n0}", points));
			m.Offset = new Vector2((float)r.NextDouble() * 50 - 50, r.Next(0, 2) == 0 ? -20 : 20);
			scoreMessages.Enqueue(m);
		}

		public static void DodgeCompleted()
		{
			Console.WriteLine("NICE DODGE!");
			WriteMessage("NICE DODGE!");
			AddScore(10000);
		}

		private static void WriteMessage(String message)
		{
				Message m = new Message(message);
				messages.Enqueue(m);
		}

		private void UpdateMessageQueue(GameTime gameTime, Queue<Message> queue)
		{
			foreach (Message m in queue)
			{
				if (m.Expiration < 0)
				{
					m.Expiration = gameTime.TotalGameTime.TotalSeconds + 2;
				}
			}

			if (queue.Count > 0 && queue.Peek().Expiration < gameTime.TotalGameTime.TotalSeconds)
			{
				queue.Dequeue();
			}
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
			Font = Content.Load<SpriteFont>("Font");

			DebugShape.TextureRectangle = Content.Load<Texture2D>("outline_square");
			DebugShape.TextureCircle = Content.Load<Texture2D>("outline_circle");

			Road = new Road(road_texture);

			TrafficManager = new TrafficManager(Content, world, 500, Road.NumLanes, Road.LaneWidth);


			player = new Player(CarType.MERCEDES, world, car_texture, 15.0f);
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

            var kstate = Keyboard.GetState();
			puck.CheckForNewPuckData();

			if (prevKeyState.IsKeyUp(Keys.Z) && kstate.IsKeyDown(Keys.Z))
			{
				DEBUG = !DEBUG;
			}

			if (prevKeyState.IsKeyUp(Keys.OemOpenBrackets) && kstate.IsKeyDown(Keys.OemOpenBrackets))
			{
				scale /= 1.5f;
			}

			if (prevKeyState.IsKeyUp(Keys.OemCloseBrackets) && kstate.IsKeyDown(Keys.OemCloseBrackets))
			{
				scale *= 1.5f;
			}

			TrafficManager.Update(gameTime, player.Position.Y);

			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

			cameraPosition = new Vector2(0, player.Position.Y + 5);
			Road.Update(player.Position.Y);
			player.Update(gameTime, kstate, puck.PuckPack0.Gyrometer[0]);

			if (!player.crashed)
			{
				score += (int)(player.Velocity.Y * 1);
			}

			UpdateMessageQueue(gameTime, messages);
			UpdateMessageQueue(gameTime, scoreMessages);


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
				TrafficManager.RenderTraffic(spriteBatch);
			}

            spriteBatch.End();

			if (DEBUG)
			{
				cameraEffect.Alpha = 0.25f;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, cameraEffect);
				player.Render(spriteBatch);
				TrafficManager.RenderTraffic(spriteBatch);
				spriteBatch.End();

				DebugView.RenderDebugData(cameraEffect.Projection, cameraEffect.View);
			}

			spriteBatch.Begin();
			spriteBatch.DrawString(Font, "Score: " + score, new Vector2(GraphicsDevice.Viewport.Width - 200, GraphicsDevice.Viewport.Height - 50), Color.Blue, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
			foreach (Message m in scoreMessages)
			{
				Color color = Color.Orange;
				spriteBatch.DrawString(Font, m.Text, new Vector2(GraphicsDevice.Viewport.Width - 100, GraphicsDevice.Viewport.Height - 40) + m.Offset, color, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
			}


			int i = 1;
			foreach (Message m in messages)
			{
				Color color = new Color(1f, 0.2f, 0.2f);
				spriteBatch.DrawString(Font, m.Text, new Vector2(50, GraphicsDevice.Viewport.Height - i * 50), color, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
				i += 1;
			}
			spriteBatch.End();

			base.Draw(gameTime);
        }
    }
}
