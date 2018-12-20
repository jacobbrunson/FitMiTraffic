using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Gui
{
    //Queue which will remove messages after their expiration date
	class MessageQueue : Queue<Message>
	{
        //Parameters
		const float defaultExpiration = 1;

		public void Update(GameTime gameTime)
		{
			foreach (Message m in this)
			{
				if (m.Expiration < 0) //A message will have a -1 expiration by default.
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
