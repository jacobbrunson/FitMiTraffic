using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Gui
{
	class Message
	{
		public String Text;
		public double Expiration;
		public Vector2 Offset;

		public Message(String text)
		{
			Text = text;
			Expiration = -1;
		}
	}

	class MessageQueue : Queue<Message>
	{
		private const float defaultExpiration = 1;

		public void Update(GameTime gameTime)
		{
			foreach (Message m in this)
			{
				if (m.Expiration < 0)
				{
					m.Expiration = gameTime.TotalGameTime.TotalSeconds + defaultExpiration;
				}
			}

			if (Count > 0 && Peek().Expiration < gameTime.TotalGameTime.TotalSeconds)
			{
				Dequeue();
			}
		}

	}
}
