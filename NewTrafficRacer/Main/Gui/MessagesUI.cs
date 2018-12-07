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

	class MessagesUI
	{
		private static SpriteFont Font;

		private MessageQueue messages = new MessageQueue();

		public void WriteMessage(String message, int x)
		{
			var m = new Message(message);
			m.Offset = new Vector2(x, 0);
			messages.Enqueue(m);
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
				Color color = Color.Yellow;
				Vector2 d = Font.MeasureString(m.Text);
				spriteBatch.DrawString(Font, m.Text, new Vector2(250, 700) + m.Offset, color, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
			}
		}

		public static void LoadContent(ContentManager content)
		{
            Console.WriteLine("tryna load gamefont");

            Font = content.Load<SpriteFont>("GameFont");
		}
	}
}
