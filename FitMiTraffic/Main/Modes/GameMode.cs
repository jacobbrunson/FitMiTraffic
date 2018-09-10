using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitMiTraffic.Main.Environment;
using FitMiTraffic.Main.Graphics;
using FitMiTraffic.Main.Gui;
using FitMiTraffic.Main.Input;
using FitMiTraffic.Main.Vehicle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics;

namespace FitMiTraffic.Main.Modes
{
	public class GameMode : Mode
	{
		//Parameters
		private const int dodgePoints = 10000;
		private Vector3 initialLightPosition = new Vector3(20, 20, 20);
		private Vector3 lightDirection = new Vector3(-1, -1, -1);
		private Vector4 ambientColor = new Vector4(0.9f, 0.9f, 1, 1);
		private const float ambientIntensity = 0.8f;
		private const float diffuseIntensity = 1;

		//Things
		private World world;
		private Player player;
		private EnvironmentManager environment;
		private TrafficManager trafficManager;

		//Graphics
		private Camera camera;
		private Lighting lighting;
		private BaseEffect effect;
		private PostProcessor postProcessor;
		private DebugView debugView;

		//GUI
		private MessagesUI messagesUI;
		private ScoreUI scoreUI;
		private GameOverUI gameOverUI;

		//State
		private int score;
		private GameState state;
		private double stateChangeTime;


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

			player = new Player(content, CarType.SPORT, world, 20);
			player.DodgeCompleteCallback = DodgeCompleted;

			environment = new EnvironmentManager(content, world);
			trafficManager = new TrafficManager(content, world, 1000, Road.NumLanes, Road.LaneWidth);

			camera = new Camera(graphics.Viewport.Width, graphics.Viewport.Height);
			lighting = new Lighting(graphics);
			lighting.Position = initialLightPosition;
			lighting.Direction = lightDirection;

			effect = new BaseEffect(content.Load<Effect>("effect"));
			postProcessor = new PostProcessor(graphics, spriteBatch, content.Load<Effect>("desaturate"));

			messagesUI = new MessagesUI();
			scoreUI = new ScoreUI();
			gameOverUI = new GameOverUI();
		}

		private void HandleInput(GameTime gameTime)
		{
			InputManager.Update();

			if (InputManager.Escape)
			{
				game.Exit();
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

			//Step forward time (slo-mo if player crashed)
			float timeScale = 1.0f;
			if (player.crashed)
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
					player.Velocity = new Vector2(player.Velocity.X, 20);
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
			environment.Update(gameTime, player.Position.Y);
			player.Update(gameTime, InputManager.LateralMovement);

			//Light & camera follow player
			camera.Target = new Vector2(0, player.Position.Y);
			lighting.Position = new Vector3(lighting.Position.X, player.Position.Y + 15, lighting.Position.Z);

			//Update GUI
			messagesUI.Update(gameTime);
			scoreUI.Update(gameTime, score);
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
			effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);
			effect.Parameters["AmbientColor"].SetValue(ambientColor);
			effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

			//Render shadow map
			graphics.SetRenderTarget(lighting.ShadowMap);
			graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
			effect.Technique = effect.Techniques["ShadowMap"];

			environment.Render(gameTime, graphics, effect);
			player.Render(gameTime, effect);
			trafficManager.RenderTraffic(gameTime, effect);


			//Render scene
			graphics.SetRenderTarget(null);

			postProcessor.Begin();

			effect.View = camera.View;
			effect.Projection = camera.Projection;
			effect.ShadowMap = lighting.ShadowMap;

			effect.Technique = effect.Techniques["ShadowedScene"];
			environment.Render(gameTime, graphics, effect);

			effect.Technique = effect.Techniques["ShadowedCar"];
			player.Render(gameTime, effect);
			trafficManager.RenderTraffic(gameTime, effect);

			postProcessor.End(player.crashed);


			//Render GUI
			spriteBatch.Begin();

			messagesUI.Render(spriteBatch, graphics.Viewport.Width, graphics.Viewport.Height);
			scoreUI.Render(spriteBatch, graphics.Viewport.Width, graphics.Viewport.Height);

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
