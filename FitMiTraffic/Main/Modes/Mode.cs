using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Modes
{
	public abstract class Mode
	{

		protected TrafficGame game;
		protected GraphicsDevice graphics;
		protected SpriteBatch spriteBatch;
		protected ContentManager content;

		public Mode(TrafficGame game, GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content)
		{
			this.game = game;
			this.graphics = graphics;
			this.spriteBatch = spriteBatch;
			this.content = content;
		}

		public abstract void Update(GameTime gameTime);
		public abstract void Render(GameTime gameTime);

	}
}
