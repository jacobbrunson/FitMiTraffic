using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitMiTraffic.Main.Utility;

namespace FitMiTraffic.Main.Gui
{
	class TitleUI
	{
		private static Texture2D traffic;
		private static Texture2D racer;
		private static SpriteFont font;

		private float trafficOffset;
		private float racerOffset;

		private float scale = 0;

		public void Render(SpriteBatch spriteBatch, GameTime gameTime, int viewportWidth, int viewportHeight)
		{
			spriteBatch.Draw(traffic, new Vector2(viewportWidth / 2 - traffic.Width / 2 + trafficOffset, 30));
			spriteBatch.Draw(racer, new Vector2(viewportWidth / 2 - racer.Width / 2 + racerOffset, traffic.Height + 50));

			CenteredText("Play", spriteBatch, viewportWidth, 380, scale);
			CenteredText("Options", spriteBatch, viewportWidth, 460, scale);
			CenteredText("Exit", spriteBatch, viewportWidth, 540, scale);
		}

		private void CenteredText(string text, SpriteBatch spriteBatch, float viewportWidth, float y, float scale)
		{
			float x = viewportWidth / 2 - font.MeasureString(text).X * scale / 2;
			y = y - font.MeasureString(text).Y * scale / 2;
			spriteBatch.DrawString(font, text, new Vector2(x - 5*scale, y + 5*scale), Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
			spriteBatch.DrawString(font, text, new Vector2(x, y), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		}

		public void Update(GameTime gameTime)
		{
			float t = (float) gameTime.TotalGameTime.TotalSeconds;

			float t1 = t.Map(0, 0.5f, 0, 1, true);
			float t2 = t.Map(0.5f, 1f, 0, 1, true);
			float t3 = t.Map(1, 1.5f, 0, 1, true);

			trafficOffset = (t1 * (2 - t1)).Map(0, 1, -800, 0, true);
			racerOffset = (t2 * (2 - t2)).Map(0, 1, 800, 0, true);
			scale = (t3 * (2 - t3)).Map(0, 1, 0, 0.75f, true);
		}

		public static void LoadContent(ContentManager content)
		{
			traffic = content.Load<Texture2D>("traffic");
			racer = content.Load<Texture2D>("racer");
			font = content.Load<SpriteFont>("GameFont");
		}
	}
}
