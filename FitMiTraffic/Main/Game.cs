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

		public static Mode Mode;
		public static bool DEBUG;

		private GraphicsDeviceManager graphicsManager;
		private SpriteBatch spriteBatch;

		public TrafficGame()
		{
			graphicsManager = new GraphicsDeviceManager(this);
			graphicsManager.PreferredBackBufferWidth = 600;
			graphicsManager.PreferredBackBufferHeight = 800;
			graphicsManager.GraphicsProfile = GraphicsProfile.HiDef;
			//graphicsManager.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
			//graphicsManager.ApplyChanges();

			Content.RootDirectory = "Content";
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
