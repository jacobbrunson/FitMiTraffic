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

	class MessagesUI
	{
		private static SpriteFont Font;

		private MessageQueue messages = new MessageQueue();

		public void WriteMessage(String message)
		{
			messages.Enqueue(new Message(message));
		}

		public void Update(GameTime gameTime)
		{
			messages.Update(gameTime);
		}

		public void Render(SpriteBatch spriteBatch, int viewportWidth, int viewportHeight)
		{
			int i = 0;
			foreach (Message m in messages)
			{
				i += 1;
				Color color = Color.Red;
				spriteBatch.DrawString(Font, m.Text, new Vector2(50, viewportHeight - 50 * i), color, 0, Vector2.Zero, 3f, SpriteEffects.None, 0);
			}
		}

		public static void LoadContent(ContentManager content)
		{
			Font = content.Load<SpriteFont>("font");
		}
	}
}
