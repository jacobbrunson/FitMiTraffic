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
    class FPSUI
    {
        private static SpriteFont Font;
        private int total_frames;
        private float total_time;
        private int fps;

        public void Update(GameTime gameTime)
        {
            total_time += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (total_time >= 1000)
            {
                fps = total_frames;
                total_frames = 0;
                total_time = 0;
            }
        }

        public void Render(SpriteBatch spriteBatch, int viewportWidth, int viewportHeight)
        {
            total_frames += 1;
            spriteBatch.DrawString(Font, "FPS: " + fps, new Vector2(viewportWidth - 80, 10), Color.White, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
        }

        public static void LoadContent(ContentManager content)
        {
            Console.WriteLine("tryna load gamefont");
            Font = content.Load<SpriteFont>("GameFont");
        }

    }
}
