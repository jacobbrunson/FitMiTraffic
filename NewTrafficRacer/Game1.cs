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
using NewTrafficRacer.Modes;
using Android.Util;

namespace NewTrafficRacer
{

    public class TrafficGame : Game
    {

        public static Mode Mode;
        public static bool DEBUG;
        public static int Duration;

        private GraphicsDeviceManager graphicsManager;
        private SpriteBatch spriteBatch;

        public TrafficGame(int width, int height, string content_dir, int duration)
        {
            graphicsManager = new GraphicsDeviceManager(this);
            graphicsManager.PreferredBackBufferWidth = width;
            graphicsManager.PreferredBackBufferHeight = height;
            graphicsManager.IsFullScreen = true;
            graphicsManager.GraphicsProfile = GraphicsProfile.HiDef;
            //graphicsManager.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
            //graphicsManager.ApplyChanges();

            if (content_dir == null)
            {
                Content.RootDirectory = "Content";
            } else
            {
                Content.RootDirectory = content_dir;
            }

            Duration = duration;
        }

        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            graphicsManager.PreferMultiSampling = true;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
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

            InputManager.Initialize();

            Mode = new GameMode(this, GraphicsDevice, spriteBatch, Content);
            //Mode = new MenuMode(this, GraphicsDevice, spriteBatch, Content);
        }

        public void Play()
        {
            DEBUG = false;
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
}
