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
			foreach (Message m in messages)
			{
				Color color = Color.Orange;
				spriteBatch.DrawString(Font, m.Text, new Vector2(viewportWidth - 100, viewportHeight - 40) + m.Offset, color, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
			}
		}

		public static void LoadContent(ContentManager content)
		{
			Font = content.Load<SpriteFont>("font");
		}
	}
}
