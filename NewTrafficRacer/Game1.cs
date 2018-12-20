using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Diagnostics;
using NewTrafficRacer.Vehicle;
using NewTrafficRacer.Environment;
using NewTrafficRacer.Gui;
using NewTrafficRacer.Input;
using Android.Util;
using NewTrafficRacer.Graphics;

namespace NewTrafficRacer
{

    public class TrafficGame : Game
    {

        //Parameters
        private const int coinPoints = 1000;
        private Vector3 initialLightPosition = new Vector3(5, 5, 5);
        private Vector3 lightDirection = new Vector3(-1, -1, -1);
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
        public static Lighting lighting;
        private PostProcessor postProcessor;
        private DebugView debugView;
        private Effect effect;

        //GUI
        private MessagesUI messagesUI;
        private ScoreUI scoreUI;
        private GameOverUI gameOverUI;
        private FPSUI fpsUI;
        private CountdownUI countdownUI;
        private TitleUI titleUI;

        //State
        private int score;
        private GameState state;
        private double stateChangeTime;
        private int startTime = -1;
        private int countdown = -1;
        private bool inTargetLane = false;




        public float adjustedSpeed
        {
            get
            {
                return playerSpeed * Math.Max(0.5f, TrafficGame.Difficulty);
            }
        }




        public static bool DEBUG;
        public static int Duration;
        public static float Difficulty;

        private GraphicsDeviceManager graphicsManager;
        private SpriteBatch spriteBatch;

        public TrafficGame(int width, int height, string content_dir, int duration, float difficulty)
        {
            graphicsManager = new GraphicsDeviceManager(this);
            graphicsManager.PreferredBackBufferWidth = width;
            graphicsManager.PreferredBackBufferHeight = height;
            graphicsManager.IsFullScreen = true;
            graphicsManager.GraphicsProfile = GraphicsProfile.HiDef;

            if (content_dir == null)
            {
                Content.RootDirectory = "Content";
            } else
            {
                Content.RootDirectory = content_dir;
            }

            Duration = duration;
            Difficulty = difficulty;
        }

        protected override void Initialize()
        {
            base.Initialize();
            tainicom.Aether.Physics2D.Settings.MaxPolygonVertices = 16;
            world = new World(Vector2.Zero);
            debugView = new DebugView(world);
            debugView.AppendFlags(DebugViewFlags.DebugPanel | DebugViewFlags.PolygonPoints);
            debugView.LoadContent(GraphicsDevice, Content);

            player = new Player(Content, CarType.SPORT, world, adjustedSpeed);
            //player.DodgeCompleteCallback = DodgeCompleted;
            player.CoinGetCallback = CoinGet;

            environment = new EnvironmentManager(Content, world);
            trafficManager = new TrafficManager(Content, world, Road.NumLanes, Road.LaneWidth);

            lightDirection.Normalize();
            camera = new Camera(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            lighting = new Lighting(GraphicsDevice, shadowMapRes);
            lighting.Position = initialLightPosition;
            lighting.Direction = lightDirection;

            effect = Content.Load<Effect>("effect");
            postProcessor = new PostProcessor(GraphicsDevice, spriteBatch, Content.Load<Effect>("desaturate"));

            messagesUI = new MessagesUI();
            scoreUI = new ScoreUI();
            gameOverUI = new GameOverUI();
            fpsUI = new FPSUI();
            countdownUI = new CountdownUI();
            titleUI = new TitleUI();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Car.LoadContent(Content);
            Road.LoadContent(Content);

            MessagesUI.LoadContent(Content);
            ScoreUI.LoadContent(Content);
            GameOverUI.LoadContent(Content);
            FPSUI.LoadContent(Content);
            CountdownUI.LoadContent(Content);
            TitleUI.LoadContent(Content);

            InputManager.Initialize();

            //Mode = new GameMode(this, GraphicsDevice, spriteBatch, Content);
            //Mode = new MenuMode(this, GraphicsDevice, spriteBatch, Content);
        }

        private void EndGame()
        {
            Game.Activity.SetResult(Android.App.Result.Ok);
            Game.Activity.Finish();
        }

        private void HandleInput(GameTime gameTime)
        {
            InputManager.Update();

            if (InputManager.Quit)
            {
                EndGame();
            }

            if (InputManager.ToggleDebug)
            {
                TrafficGame.DEBUG = !TrafficGame.DEBUG;
                camera.Revolution = Vector2.Zero;
                if (camera.Mode == CameraMode.FLAT)
                {
                    camera.Mode = CameraMode.PERSPECTIVE;
                }
                else
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
            //scoreUI.ShowPoints(dodgePoints);
            //score += dodgePoints;
        }

        private void CoinGet(Body b)
        {
            scoreUI.ShowPoints(coinPoints);
            score += coinPoints;
            environment.DestroyCoin(b);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            HandleInput(gameTime);

            if (startTime == -1)
            {
                startTime = (int)gameTime.TotalGameTime.TotalSeconds;
            }

            //Countdown
            countdown = startTime + TrafficGame.Duration - (int)gameTime.TotalGameTime.TotalSeconds;
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

            //Detect if in target lane
            int lane = Road.GetLane(player.Position.X, 0.35f);
            inTargetLane = lane == environment.road.GetHighlightAtPlayerPos();
            environment.road.SetHighlightStatus(lane);

            //Step forward time (slo-mo if player crashed or countdown finished)
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
                    int deltaScore = (int)Math.Max(1, (player.Velocity.Y * gameTime.ElapsedGameTime.TotalSeconds));
                    if (inTargetLane)
                    {
                        deltaScore *= 2;
                    }
                    score += deltaScore;
                }
            }
            else if (state == GameState.STARTING)
            {
                camera.Zoom = Math.Min(1, camera.Zoom + 0.05f);
                if (gameTime.TotalGameTime.TotalSeconds - stateChangeTime > 3)
                {
                    state = GameState.RUNNING;
                    stateChangeTime = gameTime.TotalGameTime.TotalSeconds;
                    player.Velocity = new Vector2(player.Velocity.X, adjustedSpeed);
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
            titleUI.Update(gameTime);
        }

        //This is a really ugly hack that must be called before each and every model draw on Android
        public static void RENDER_FIX(Effect effect)
        {
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);

            effect.Parameters["AmbientColor"].SetValue(ambientColor);

            //lighting.Position.Normalize();
            effect.Parameters["LightPosition"].SetValue(-lighting.Direction);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);

            effect.Parameters["ShadowMap"]?.SetValue(lighting.ShadowMap);

            effect.Parameters["ChromaKeyReplace"].SetValue(new Vector4(-1, -1, -1, -1));
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GraphicsDevice.Clear(Color.Black);

            //Update lights and camera
            camera.Update();
            lighting.Update();

            //Update graphics state
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.CullClockwiseFace };

            //Set lighting parameters
            //effect.LightViewProjection = lighting.View * lighting.Projection; //COMMENTED FOR COMPILE
            //effect.Parameters["LightPosition"].SetValue(-lighting.Direction);//effect.Parameters["DiffuseLightDirection"].SetValue(lighting.Direction); //CHANGED
            //effect.Parameters["AmbientColor"].SetValue(ambientColor);

            //Render shadow map

            GraphicsDevice.SetRenderTarget(lighting.ShadowMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            effect.CurrentTechnique = effect.Techniques["ShadowMap"];

            environment.Render(gameTime, GraphicsDevice, effect);
            player.Render(gameTime, effect);
            trafficManager.RenderTraffic(gameTime, effect);


            //Render scene
            GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace };
            GraphicsDevice.SetRenderTarget(null);

            postProcessor.Begin();

            //effect.View = camera.View; //COMMENTED FOR COMPILE
            //effect.Projection = camera.Projection; //COMMENTED FOR COMPILE
            //effect.ShadowMap = lighting.ShadowMap; //COMMENTED FOR COMPILE

            RENDER_FIX(effect);

            effect.CurrentTechnique = effect.Techniques["ShadowedScene"];
            environment.Render(gameTime, GraphicsDevice, effect);

            //effect.CurrentTechnique = effect.Techniques["ShadowedCar"]; //CHANGED
            player.Render(gameTime, effect);
            trafficManager.RenderTraffic(gameTime, effect);

            postProcessor.End(player.crashed);


            //Render GUI
            spriteBatch.Begin();

            messagesUI.Render(spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            scoreUI.Render(spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, inTargetLane);
            countdownUI.Render(spriteBatch, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            titleUI.Render(spriteBatch, gameTime, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            if (player.crashed && state != GameState.RECENTERING)
            {
                gameOverUI.Render(spriteBatch, gameTime, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
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

                //DEBUG: show shadow map
                spriteBatch.Begin(0, BlendState.Opaque, SamplerState.AnisotropicClamp);
                spriteBatch.Draw(lighting.ShadowMap, new Rectangle(0, GraphicsDevice.Viewport.Height - 256, 256, 256), Color.White);
                spriteBatch.End();
            }
        }
    }

    public enum GameState
    {
        STARTING, RUNNING, RECENTERING
    }
}
