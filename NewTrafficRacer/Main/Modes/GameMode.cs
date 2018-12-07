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
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics;

namespace NewTrafficRacer.Modes
{
	public class GameMode : Mode
	{
		//Parameters
		private const int dodgePoints = 10000;
		private Vector3 initialLightPosition = new Vector3(20, 20, 20);
        private Vector3 lightDirection = new Vector3(1, 1, 1);
        private static Vector4 ambientColor = new Vector4(0.48f, 0.54f, 0.6f, 1f);
        private static Vector4 diffuseColor = new Vector4(1f, 0.8f, 0.8f, 1f);
        private const float diffuseIntensity = 1;
        private const int shadowMapRes = 2048;
        private const float playerSpeed = 20;

        //Things
        private World world;
		private Player player;
		private EnvironmentManager environment;
		private TrafficManager trafficManager;

		//Graphics
		private static Camera camera;
		private static Lighting lighting;
		private PostProcessor postProcessor;
		private DebugView debugView;
        private Effect effect;

		//GUI
		private MessagesUI messagesUI;
		private ScoreUI scoreUI;
		private GameOverUI gameOverUI;
        private FPSUI fpsUI;
        private CountdownUI countdownUI;

		//State
		private int score;
		private GameState state;
		private double stateChangeTime;
        private int startTime = -1;
        private int countdown = -1;


		public GameMode(TrafficGame game, GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content) : base(game, graphics, spriteBatch, content)
		{
			this.graphics = graphics;
			this.spriteBatch = spriteBatch;
			this.game = game;

            tainicom.Aether.Physics2D.Settings.MaxPolygonVertices = 16;
			world = new World(Vector2.Zero);
			debugView = new DebugView(world);
			debugView.AppendFlags(DebugViewFlags.DebugPanel | DebugViewFlags.PolygonPoints);
			debugView.LoadContent(graphics, content);

            player = new Player(content, CarType.SPORT, world, playerSpeed);
			player.DodgeCompleteCallback = DodgeCompleted;

            environment = new EnvironmentManager(content, world);
			trafficManager = new TrafficManager(content, world, 1000, Road.NumLanes, Road.LaneWidth);

            lightDirection.Normalize();
            camera = new Camera(graphics.Viewport.Width, graphics.Viewport.Height);
			lighting = new Lighting(graphics, shadowMapRes);
			lighting.Position = initialLightPosition;
			lighting.Direction = lightDirection;

            effect = content.Load<Effect>("effect");
			postProcessor = new PostProcessor(graphics, spriteBatch, content.Load<Effect>("desaturate"));

            messagesUI = new MessagesUI();
			scoreUI = new ScoreUI();
			gameOverUI = new GameOverUI();
            fpsUI = new FPSUI();
            countdownUI = new CountdownUI();
		}

        private void EndGame()
        {
            Game.Activity.SetResult(Android.App.Result.Ok);
            Game.Activity.Finish();
        }

		private void HandleInput(GameTime gameTime)
		{
			InputManager.Update();

			if (InputManager.Quit) {
                EndGame();
			}

			if (InputManager.ToggleDebug)
			{
				TrafficGame.DEBUG = !TrafficGame.DEBUG;
				camera.Revolution = Vector2.Zero;
				if (camera.Mode == CameraMode.FLAT)
				{
					camera.Mode = CameraMode.PERSPECTIVE;
				} else
				{
					camera.Mode = CameraMode.FLAT;
				}
			}

			if (InputManager.ZoomOut)
			{
				camera.Scale /= 1.5f;
			}

			if (InputManager.ZoomIn)
			{
				camera.Scale *= 1.5f;
			}

			if (InputManager.Restart)
			{
				player.Recenter(gameTime);
				state = GameState.RECENTERING;
				stateChangeTime = gameTime.TotalGameTime.TotalSeconds;
			}

			camera.Revolution += InputManager.MoveCameraAmount / 30;
		}

		private void DodgeCompleted(Body b)
		{
			messagesUI.WriteMessage("+1000", (int)b.Position.X * 60); //TODO: 60 should really not be a magic constant
			scoreUI.ShowPoints(dodgePoints);
			score += dodgePoints;
		}

		public override void Update(GameTime gameTime)
		{
			HandleInput(gameTime);

            if (startTime == -1)
            {
                startTime = (int) gameTime.TotalGameTime.TotalSeconds;
            }

            countdown = startTime + TrafficGame.Duration - (int) gameTime.TotalGameTime.TotalSeconds;
            countdownUI.Update(countdown);
            if (countdown <= 5)
            {
                Console.WriteLine("COUNTDOWN: " + countdown);
                if (countdown == -3)
                {
                    Console.WriteLine("GAME OVER");
                    EndGame();
                }
            }

			//Step forward time (slo-mo if player crashed)
			float timeScale = 1.0f;
			if (player.crashed || countdown <= 0)
			{
				timeScale = 0.25f;
			}
			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds * timeScale);

			//TODO BETTER STATE SYSTEM
			if (state == GameState.RUNNING)
			{
				if (!player.crashed)
				{
					score += (int)(player.Velocity.Y * 0.5f);
				}
			}
			else if (state == GameState.STARTING)
			{
				camera.Zoom = Math.Min(1, camera.Zoom + 0.05f);
				if (gameTime.TotalGameTime.TotalSeconds - stateChangeTime > 3)
				{
					state = GameState.RUNNING;
					stateChangeTime = gameTime.TotalGameTime.TotalSeconds;
					player.Velocity = new Vector2(player.Velocity.X, playerSpeed);
				}
			}
			else if (state == GameState.RECENTERING)
			{
				camera.Zoom = Math.Max(0, camera.Zoom - 0.05f);
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

			//Update game stuff
			trafficManager.Update(gameTime, player, state);
			environment.Update(gameTime, player);
			player.Update(gameTime, InputManager.LateralMovement);

			//Light & camera follow player
			camera.Target = new Vector2(player.Position.X, player.Position.Y);
			lighting.Position = new Vector3(lighting.Position.X, player.Position.Y + 15, lighting.Position.Z);

			//Update GUI
			messagesUI.Update(gameTime);
			scoreUI.Update(gameTime, score);
            fpsUI.Update(gameTime);
		}

        //This is a really ugly hack that must be called before each and every model draw on Android
        public static void RENDER_FIX(Effect effect)
        {
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);

            effect.Parameters["AmbientColor"].SetValue(ambientColor);

            //lighting.Position.Normalize();
            effect.Parameters["LightPosition"].SetValue(lighting.Direction);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);

            //effect.Parameters["ShadowMap"].SetValue(lighting.ShadowMap);

            effect.Parameters["ChromaKeyReplace"].SetValue(new Vector4(-1, -1, -1, -1));

            effect.CurrentTechnique = effect.Techniques["ShadowedScene"];
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
            //effect.LightViewProjection = lighting.View * lighting.Projection; //COMMENTED FOR COMPILE
            effect.Parameters["LightPosition"].SetValue(lighting.Direction);//effect.Parameters["DiffuseLightDirection"].SetValue(lighting.Direction); //CHANGED
			//effect.Parameters["AmbientColor"].SetValue(ambientColor);

            //Render shadow map
            /*
            graphics.SetRenderTarget(lighting.ShadowMap);
			graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
			effect.CurrentTechnique = effect.Techniques["ShadowMap"];

			environment.Render(gameTime, graphics, effect);
			player.Render(gameTime, effect);
			trafficManager.RenderTraffic(gameTime, effect);
            */

			//Render scene
			graphics.SetRenderTarget(null);

			postProcessor.Begin();

            //effect.View = camera.View; //COMMENTED FOR COMPILE
            //effect.Projection = camera.Projection; //COMMENTED FOR COMPILE
            //effect.ShadowMap = lighting.ShadowMap; //COMMENTED FOR COMPILE

            RENDER_FIX(effect);

            effect.CurrentTechnique = effect.Techniques["ShadowedScene"];
			environment.Render(gameTime, graphics, effect);

			//effect.CurrentTechnique = effect.Techniques["ShadowedCar"]; //CHANGED
			player.Render(gameTime, effect);
			trafficManager.RenderTraffic(gameTime, effect);

            postProcessor.End(player.crashed);


			//Render GUI
			spriteBatch.Begin();

			messagesUI.Render(spriteBatch, graphics.Viewport.Width, graphics.Viewport.Height);
			scoreUI.Render(spriteBatch, graphics.Viewport.Width, graphics.Viewport.Height);
            countdownUI.Render(spriteBatch, graphics.Viewport.Width, graphics.Viewport.Height);

			if (player.crashed && state != GameState.RECENTERING)
			{
				gameOverUI.Render(spriteBatch, gameTime, graphics.Viewport.Width, graphics.Viewport.Height);
			}

			spriteBatch.End();
            
            //Render debug
            if (TrafficGame.DEBUG)
			{
				trafficManager.RenderDebug(debugView, camera.View, camera.Projection);
                debugView.RenderDebugData(camera.Projection, camera.View, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, 0.8f);
                spriteBatch.Begin();
                fpsUI.Render(spriteBatch, 600, 800);
                spriteBatch.End();
            }

			//DEBUG: render shadow map
			//spriteBatch.Begin(0, BlendState.Opaque, SamplerState.AnisotropicClamp);
			//spriteBatch.Draw(lighting.ShadowMap, new Rectangle(0, 0, 600, 800), Color.White);
			//spriteBatch.End();
		}
	}

	public enum GameState
	{
		STARTING, RUNNING, RECENTERING
	}
}
