using FitMiTraffic.Main.Graphics;
using FitMiTraffic.Main.Vehicle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Environment
{
    class Coin : RenderedModel
    {
        private const string ModelName = "coin";

        public Coin(ContentManager content) : base(content, ModelName)
        {
            this.Rotation = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, MathHelper.PiOver2, 0);
            this.Size = new Vector3(1, 1, 0.2f);
            this.Offset = new Vector3(0, 0, 0.75f);
        }

        public void Update(GameTime gameTime, Player player)
        {
            //if (player.Position)
            this.Rotation *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
