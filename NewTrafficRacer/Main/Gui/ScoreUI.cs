using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Gui
{
	class ScoreUI
	{
		private static SpriteFont Font;

		private int Score;
		private MessageQueue points = new MessageQueue();

		public void ShowPoints(int amount)
		{
			Random r = new Random();

			Message m = new Message(String.Format("+{0:n0}", amount));
			m.Offset = new Vector2((float)r.NextDouble() * 100, r.Next(0, 2) == 0 ? -15 : 50);

			points.Enqueue(m);
		}

		public void Update(GameTime gameTime, int score)
		{
			Score = score;
			points.Update(gameTime);
		}

		public void Render(SpriteBatch spriteBatch, int viewportWidth, int viewportHeight, bool inTargetLane)
		{
			string s = Score.ToString();
			Vector2 d = Font.MeasureString(s);
			Vector2 scorePos = new Vector2(20, 10);
            Color c = Color.White;
            if (inTargetLane)
            {
                c = Color.Gold;
            }
            spriteBatch.DrawString(Font, s, scorePos + new Vector2(-3, 3), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.DrawString(Font, s, scorePos, c, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

            foreach (Message m in points)
			{
                Color color = Color.Gold;
                spriteBatch.DrawString(Font, m.Text, scorePos + m.Offset + new Vector2(-2, 2), Color.Black, 0, Vector2.Zero, 0.4f, SpriteEffects.None, 0);
                spriteBatch.DrawString(Font, m.Text, scorePos + m.Offset, color, 0, Vector2.Zero, 0.4f, SpriteEffects.None, 0);
			}
		}

		public static void LoadContent(ContentManager content)
		{
            Console.WriteLine("tryna load gamefont");

            Font = content.Load<SpriteFont>("GameFont");
		}

	}
}
