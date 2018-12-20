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
using NewTrafficRacer.Main.Graphics;

namespace NewTrafficRacer
{

    public class TrafficGame : Game
    {

        //Parameters
        const int coinPoints = 1000;
        
        const float diffuseIntensity = 1;
        const float playerSpeed = 20;

        //Things
        World world;
        Player player;
        EnvironmentManager environment;
        TrafficManager trafficManager;

        //Graphics
        PostProcessor postProcessor;
        DebugView debugView;
        Effect effect;

        //GUI
        MessagesUI messagesUI;
        ScoreUI scoreUI;
        GameOverUI gameOverUI;
        FPSUI fpsUI;
        CountdownUI countdownUI;
        TitleUI titleUI;

        //State
        int score;
        GameState state;
        double stateChangeTime;
        int startTime = -1;
        int countdown = -1;
        bool inTargetLane = false;


        public static GraphicsDevice Graphics;

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

        GraphicsDeviceManager graphicsManager;
        SpriteBatch spriteBatch;









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
            Graphics = GraphicsDevice;

            tainicom.Aether.Physics2D.Settings.MaxPolygonVertices = 16;
            world = new World(Vector2.Zero);
            debugView = new DebugView(world);
            debugView.AppendFlags(DebugViewFlags.DebugPanel | DebugViewFlags.PolygonPoints);
            debugView.LoadContent(GraphicsDevice, Content);

            player = new Player(Content, CarType.SPORT, world, adjustedSpeed);
            player.DodgeCompleteCallback = DodgeCompleted;
            player.CoinGetCallback = CoinGet;

            environment = new EnvironmentManager(Content, world);
            trafficManager = new TrafficManager(Content, world, Road.NumLanes, Road.LaneWidth);

            Lighting.Initialize();

            effect = Content.Load<Effect>("effect");
            postProcessor = new PostProcessor(GraphicsDevice, spriteBatch, Content.Load<Effect>("desaturate"));

            messagesUI = new MessagesUI();
            scoreUI = new ScoreUI();
            gameOverUI = new GameOverUI();
            fpsUI = new FPSUI();
            countdownUI = new CountdownUI();
            titleUI = new TitleUI();

            InputManager.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Car.LoadContent(Content);

            MessagesUI.LoadContent(Content);
            ScoreUI.LoadContent(Content);
            GameOverUI.LoadContent(Content);
            FPSUI.LoadContent(Content);
            CountdownUI.LoadContent(Content);
            TitleUI.LoadContent(Content);
        }

        void EndGame()
        {
            Game.Activity.SetResult(Android.App.Result.Ok);
            Game.Activity.Finish();
        }

        void HandleInput(GameTime gameTime)
        {
            InputManager.Update();

            if (InputManager.Quit)
            {
                EndGame();
            }

            if (InputManager.ToggleDebug)
            {
                TrafficGame.DEBUG = !TrafficGame.DEBUG;
                Camera.main.Revolution = Vector2.Zero;
                if (Camera.main.Mode == CameraMode.FLAT)
                {
                    Camera.main.Mode = CameraMode.PERSPECTIVE;
                }
                else
                {
                    Camera.main.Mode = CameraMode.FLAT;
                }
            }

            if (InputManager.ZoomOut)
            {
                Camera.main.Scale /= 1.5f;
            }

            if (InputManager.ZoomIn)
            {
                Camera.main.Scale *= 1.5f;
            }

            if (InputManager.Restart)
            {
                player.Recenter(gameTime);
                state = GameState.RECENTERING;
                stateChangeTime = gameTime.TotalGameTime.TotalSeconds;
            }

            Camera.main.Revolution += InputManager.MoveCameraAmount / 30;
        }

        void DodgeCompleted(Body b)
        {
            //scoreUI.ShowPoints(dodgePoints);
            //score += dodgePoints;
        }

        void CoinGet(Body b)
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
                if (countdown == -3)
                {
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
                Camera.main.Zoom = Math.Min(1, Camera.main.Zoom + 0.05f);
                if (gameTime.TotalGameTime.TotalSeconds - stateChangeTime > 3)
                {
                    state = GameState.RUNNING;
                    stateChangeTime = gameTime.TotalGameTime.TotalSeconds;
                    player.Velocity = new Vector2(player.Velocity.X, adjustedSpeed);
                }
            }
            else if (state == GameState.RECENTERING)
            {
                Camera.main.Zoom = Math.Max(0, Camera.main.Zoom - 0.05f);
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
            Camera.main.Target = new Vector2(player.Position.X, player.Position.Y);
            Lighting.Position = new Vector3(Lighting.Position.X, player.Position.Y + 15, Lighting.Position.Z);

            //Update GUI
            messagesUI.Update(gameTime);
            scoreUI.Update(gameTime, score);
            fpsUI.Update(gameTime);
            titleUI.Update(gameTime);
        }

        

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GraphicsDevice.Clear(Color.Black);

            //Update lights and camera
            Camera.main.Update();
            Lighting.Update();

            //Update graphics state
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.CullClockwiseFace };

            //Render shadow map

            GraphicsDevice.SetRenderTarget(Lighting.ShadowMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            effect.CurrentTechnique = effect.Techniques["ShadowMap"];

            environment.Render(gameTime, GraphicsDevice, effect);
            player.Render(gameTime, effect);
            trafficManager.RenderTraffic(gameTime, effect);


            //Render scene
            GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace };
            GraphicsDevice.SetRenderTarget(null);

            postProcessor.Begin();

            RenderHack.RENDER_FIX(effect);

            effect.CurrentTechnique = effect.Techniques["ShadowedScene"];
            environment.Render(gameTime, GraphicsDevice, effect);

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
                trafficManager.RenderDebug(debugView, Camera.main.View, Camera.main.Projection);
                debugView.RenderDebugData(Camera.main.Projection, Camera.main.View, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, 0.8f);
                spriteBatch.Begin();
                fpsUI.Render(spriteBatch, 600, 800);
                spriteBatch.End();

                //DEBUG: show shadow map
                spriteBatch.Begin(0, BlendState.Opaque, SamplerState.AnisotropicClamp);
                spriteBatch.Draw(Lighting.ShadowMap, new Rectangle(0, GraphicsDevice.Viewport.Height - 256, 256, 256), Color.White);
                spriteBatch.End();
            }
        }
    }

    public enum GameState
    {
        STARTING, RUNNING, RECENTERING
    }
}
