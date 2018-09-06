using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Gui
{
	class GameOverUI
	{
		private static Texture2D ouch;
		private static Texture2D retry;

		public void Render(SpriteBatch spriteBatch, GameTime gameTime, int viewportWidth, int viewportHeight)
		{
			spriteBatch.Draw(ouch, new Vector2(viewportWidth / 2 - ouch.Width / 2, 200));
			if ((int)gameTime.TotalGameTime.TotalMilliseconds/500 % 2 == 1)
			{
				spriteBatch.Draw(retry, new Vector2(viewportWidth / 2 - retry.Width / 2, 200 + ouch.Height + 20));
			}
		}

		public static void LoadContent(ContentManager content)
		{
			ouch = content.Load<Texture2D>("ouch");
			retry = content.Load<Texture2D>("retry");
		}
	}
}
