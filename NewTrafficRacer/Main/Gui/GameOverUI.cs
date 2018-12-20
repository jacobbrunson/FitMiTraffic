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
	class GameOverUI
	{
        //Parameters
        const string ouchTexName = "ouch";
        const string retryTexName = "retry";

        //State
		static Texture2D ouch;
		static Texture2D retry;

		public void Render(SpriteBatch spriteBatch, GameTime gameTime)
		{
            int viewportWidth = TrafficGame.Graphics.Viewport.Width;
            int viewportHeight = TrafficGame.Graphics.Viewport.Height;

            spriteBatch.Draw(ouch, new Vector2(viewportWidth / 2 - ouch.Width / 2, 200));
			if ((int)gameTime.TotalGameTime.TotalMilliseconds/500 % 2 == 1)
			{
				spriteBatch.Draw(retry, new Vector2(viewportWidth / 2 - retry.Width / 2, 200 + ouch.Height + 20));
			}
		}

		public static void LoadContent(ContentManager content)
		{
            ouch = content.Load<Texture2D>(ouchTexName);
			retry = content.Load<Texture2D>(retryTexName);
		}
	}
}
