﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Gui
{
    class CountdownUI
    {
        //Parameters
        const string finalText = "GREAT WORKOUT";
        const string fontName = "GameFont";
        const int threshold = 10;

        //State
        static SpriteFont Font;
        int countdown;

        public void Update(int countdown)
        {
            this.countdown = countdown;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            int viewportWidth = TrafficGame.Graphics.Viewport.Width;
            int viewportHeight = TrafficGame.Graphics.Viewport.Height;

            if (countdown > threshold)
            {
                return;
            }

            string s;

            if (countdown <= 0)
            {
                s = finalText;
            } else
            {
                s = countdown.ToString();
            }

            Vector2 d = Font.MeasureString(s);

            Vector2 pos = new Vector2(viewportWidth / 2 - d.X / 2, viewportHeight - d.Y - 30);

            spriteBatch.DrawString(Font, s, pos, Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.DrawString(Font, s, pos - new Vector2(4, 4), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        public static void LoadContent(ContentManager content)
        {
            Font = content.Load<SpriteFont>(fontName);
        }

    }
}
