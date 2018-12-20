using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using NewTrafficRacer.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace NewTrafficRacer.Environment
{
    class Coin : RenderedModel
    {
        const string modelName = "coin";
        const string texName = "CoinTex";

        World world;
        public Body Body;

        public Coin(ContentManager content, World world, Vector2 position) : base(content, modelName, texName)
        {
            this.Position = new Vector3(position.X, position.Y, 0);
            this.Rotation = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, MathHelper.PiOver2, 0);
            this.Size = new Vector3(1, 1, 0.2f);
            this.Offset = new Vector3(0, 0, 0.75f);
            this.world = world;

            Body = world.CreateBody(position, 0, BodyType.Kinematic);
            Body.CreateRectangle(Size.X, Size.Z, 1, Vector2.Zero);
            Body.FixedRotation = true;
            Body.SetIsSensor(true);
        }

        public void Update(GameTime gameTime)
        {
            float deltaTheta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rotation *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, deltaTheta);
            Body.Rotation += deltaTheta;
        }
    }
}
