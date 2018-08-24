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

namespace FitMiTraffic
{
	class CastedRay
	{
		public readonly Vector2 P1;
		public readonly Vector2 P2;
		public readonly bool Hit;

		public CastedRay(Vector2 p1, Vector2 p2, bool hit)
		{
			P1 = p1;
			P2 = p2;
			Hit = hit;
		}
	}

	class Car
	{
		public Body Body;

		private World World;
		private Texture2D texture;
		private CarType Type;

		private float DesiredSpeed;

		public int Lane = -1;

		private CastedRay Ray;

		public Vector2 Position
		{
			get { return Body.Position; }
			set { Body.Position = value; }
		}

		public Vector2 Velocity
		{
			get { return Body.LinearVelocity; }
			set {
				Body.LinearVelocity = new Vector2(value.X, Body.LinearVelocity.Y);
				DesiredSpeed = value.Y;
			}
		}


		private Vector2 scale;
		private Vector2 textureSizePx;
		public Vector2 BodySize;

		public Car(CarType type, World world, Texture2D texture, float initialSpeed)
		{
			this.World = world;
			this.texture = texture;
			Type = type;
			DesiredSpeed = initialSpeed;

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
			Body.LinearVelocity = new Vector2(0, initialSpeed);
		}

		public Body AnticipateCollision(float maxDistance)
		{
			Vector2 p1 = new Vector2(Position.X, Position.Y + BodySize.Y / 2);
			Vector2 p2 = p1 + Vector2.UnitY * maxDistance;
			List<Fixture> hits = World.RayCast(p1, p2);

			bool hit = hits.Count > 0;

			Ray = new CastedRay(p1, p2, hit);

			//anticipated collision
			if (hit)
			{
				return hits[0].Body;
				//car.Velocity = new Vector2(0, Math.Min(car.Velocity.Y, hits[0].Body.LinearVelocity.Y - 1));
			}
			return null;
		}

		public void Update(GameTime gameTime)
		{
			if (Math.Abs(Velocity.Y - DesiredSpeed) > 0.1)
			{
				
				Body.LinearVelocity = new Vector2(Velocity.X, Velocity.Y * 0.9f + DesiredSpeed * 0.1f);
			}
		}

		public void Render(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(texture, Body.Position, null, Color.White, Body.Rotation,
			textureSizePx / 2, scale, SpriteEffects.FlipVertically, 0.0f);

			if (Ray != null)
			{
				TrafficGame.DebugView.BeginCustomDraw(TrafficGame.cameraEffect.Projection, TrafficGame.cameraEffect.View);

				if (Ray.Hit)
				{
					TrafficGame.DebugView.DrawPoint(Ray.P1, .25f, new Color(0.9f, 0.4f, 0.4f));
					TrafficGame.DebugView.DrawSegment(Ray.P2, Ray.P1, new Color(0.8f, 0.4f, 0.4f));
				}
				else
				{
					TrafficGame.DebugView.DrawPoint(Ray.P1, .25f, new Color(0.4f, 0.9f, 0.4f));
					TrafficGame.DebugView.DrawSegment(Ray.P2, Ray.P1, new Color(0.8f, 0.8f, 0.8f));
				}
				TrafficGame.DebugView.EndCustomDraw();
			}
		}
	}
}
