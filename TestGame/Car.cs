using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Common;

namespace TestGame
{
	class Car
	{
		public Body Body;

		private World World;
		private Texture2D texture;
		private CarType Type;

		public int Lane = -1;

		public Vector2 Position
		{
			get { return Body.Position; }
			set { Body.Position = value; }
		}

		public Vector2 Velocity
		{
			get { return Body.LinearVelocity; }
			set { Body.LinearVelocity = value; }
		}


		private Vector2 scale;
		private Vector2 textureSizePx;
		public Vector2 BodySize;

		public Car(CarType type, World world, Texture2D texture)
		{
			this.World = world;
			this.texture = texture;
			Type = type;

			var textureSize = new Vector2(type.Width, type.Height);
			textureSizePx = new Vector2(texture.Width, texture.Height);

			scale = textureSize / textureSizePx;

			BodySize = new Vector2(type.Width - type.OffsetX * 2 * scale.X, type.Height - type.OffsetY * 2 * scale.Y);

			float rTop = Type.RadiusTop * scale.Y;
			float rBot = Type.RadiusBottom * scale.Y;

			Vertices verts = new Vertices();
			int steps = 4;
			float angleStep = MathHelper.Pi / steps;

			//Top curve
			for (int i = 0; i <= steps; i++)
			{
				float angle = angleStep * i;
				verts.Add(new Vector2((float)Math.Cos(angle) * BodySize.X / 2, (float)Math.Sin(angle) * rTop + (BodySize.Y/2 - rTop)));
			}

			//Bottom curve
			for (int i = 0; i <= steps; i++)
			{
				float angle = angleStep * i + MathHelper.Pi;
				verts.Add(new Vector2((float)Math.Cos(angle) * BodySize.X / 2, (float)Math.Sin(angle) * rBot - (BodySize.Y / 2 - rBot)));
			}

			Body = world.CreateBody();
			Body.CreatePolygon(verts, 1f);
			Body.BodyType = BodyType.Dynamic;
			Body.AngularDamping = 0.5f;
		}

		public void Render(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(texture, Body.Position, null, Color.White, Body.Rotation,
			textureSizePx / 2, scale, SpriteEffects.FlipVertically, 0.0f);
		}
	}
}
