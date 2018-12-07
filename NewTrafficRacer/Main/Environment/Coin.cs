using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using NewTrafficRacer.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Environment
{
    class Coin : RenderedModel
    {
        private const string ModelName = "coin";

        public Coin(ContentManager content) : base(content, ModelName, "CoinTex")
        {
            this.Rotation = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, MathHelper.PiOver2, 0);
            this.Size = new Vector3(1, 1, 0.2f);
            this.Offset = new Vector3(0, 0, 0.75f);
        }

        public void Update(GameTime gameTime)
        {
            this.Rotation *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
