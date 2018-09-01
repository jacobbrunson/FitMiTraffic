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
using Microsoft.Xna.Framework.Content;
using FitMiTraffic.Main.Environment;
using FitMiTraffic.Main.Utility;
using FitMiTraffic.Main.Graphics;

namespace FitMiTraffic.Main.Vehicle
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

	enum CarState { Driving, LookingRight, MovingRight, LookingLeft, MovingLeft, Merging, Exiting }

	class Car
	{
		public Body Body;

		private World World;
		private CarType Type;

		private float DesiredSpeed;
		private float InitialSpeed;

		public int DesiredLane = 0;
		private int _Lane;
		private int PreviousLane = -1;

		public RenderedModel model;

		public int Lane
		{
			get { return _Lane; }
			set
			{
				PreviousLane = Lane;
				_Lane = value;
			}
		}

		public List<CastedRay> Rays = new List<CastedRay>();

		public CarState State = CarState.Driving;

		private double StateChangeTime;

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

		public Car(ContentManager content, CarType type, World world, float initialSpeed)
		{
			this.World = world;
			Type = type;
			DesiredSpeed = initialSpeed;
			InitialSpeed = initialSpeed;

			model = new RenderedModel(content, type.ModelName, type.TextureName);

			var textureSize = new Vector2(type.Width, type.Height);
			textureSizePx = new Vector2(type.Texture.Width, type.Texture.Height);

			scale = textureSize / textureSizePx;

			BodySize = new Vector2(type.Width, type.Height);

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

		private Body RayCast(Vector2 p1, Vector2 p2)
		{
			List<Fixture> hits = World.RayCast(p1, p2);

			bool hit = hits.Count > 0;

			Rays.Add(new CastedRay(p1, p2, hit));

			if (hit)
			{
				return hits[0].Body;
			}
			return null;
		}

		public Body AnticipateCollision(float maxDistance)
		{
			int numRays = 4;

			Body[] bodies = new Body[numRays];

			for (int i = 0; i < numRays; i++)
			{
				Vector2 p1 = new Vector2(Position.X + i.Map(0, 3, -Type.Width / 2, Type.Width / 2), Position.Y + BodySize.Y / 2);
				Vector2 p2 = p1 + Vector2.UnitY * maxDistance;

				bodies[i] = RayCast(p1, p2);
			}

			for (int i = 0; i < numRays; i++)
			{
				if (bodies[i] != null)
				{
					return bodies[i];
				}
			}

			return null;
		}

		public bool IsSafeToMerge(Vector2 direction, float buffer=1)
		{
			//Note: This method should really use short-circuit evaluation, but I don't because it makes the debug view look cooler
			int numRays = 4;

			Body[] bodies = new Body[numRays];

			for (int i = 0; i < numRays; i++)
			{
				Vector2 p1 = Position + direction * BodySize.X / 2 + Vector2.UnitY * BodySize.Y * i.Map(0, 3, -buffer, buffer);
				Vector2 p2 = p1 + direction * Road.LaneWidth;

				bodies[i] = RayCast(p1, p2);
			}

			for (int i = 0; i < numRays; i++)
			{
				if (bodies[i] != null)
				{
					return false;
				}
			}

			return true;
		}

		private void ChangeState(CarState state, GameTime gameTime)
		{
			State = state;
			StateChangeTime = gameTime.TotalGameTime.TotalSeconds;
		}

		public void Update(GameTime gameTime)
		{
			Random random = new Random((int)gameTime.TotalGameTime.TotalMilliseconds + (int)State + (int)(Velocity.Y*100) + GetHashCode());

			if (State == CarState.Driving)
			{
				double d = random.NextDouble();
				if (d < gameTime.ElapsedGameTime.TotalSeconds * 0.05f)
				{
					int i = random.Next(0, 2);
					if (Lane != Road.NumLanes - 1 && i == 0)
					{
						ChangeState(CarState.LookingRight, gameTime);

					} else if (Lane != 0 && i == 1)
					{
						ChangeState(CarState.LookingLeft, gameTime);
						Velocity = new Vector2(Velocity.X, Velocity.Y + random.NextDouble().Map(0, 1, 0.5f, 2.0f));
					}
				}
			} else if (State == CarState.LookingRight)
			{
				bool isSafe = IsSafeToMerge(Vector2.UnitX);
				if (gameTime.TotalGameTime.TotalSeconds - StateChangeTime > 2.5 && isSafe)
				{
					ChangeState(CarState.MovingRight, gameTime);
					DesiredLane = Lane + 1;
				}
				else if (gameTime.TotalGameTime.TotalSeconds - StateChangeTime > 5)
				{
					ChangeState(CarState.Driving, gameTime);
				}
			} else if (State == CarState.MovingRight)
			{
				if (Lane != PreviousLane)
				{
					ChangeState(CarState.Driving, gameTime);
				}
			}
			else if (State == CarState.LookingLeft)
			{
				bool isSafe = IsSafeToMerge(-Vector2.UnitX, 1.5f);
				if (gameTime.TotalGameTime.TotalSeconds - StateChangeTime > 2.5 && isSafe)
				{
					ChangeState(CarState.MovingLeft, gameTime);
					DesiredLane = Lane - 1;
				}
				else if (gameTime.TotalGameTime.TotalSeconds - StateChangeTime > 5)
				{
					ChangeState(CarState.Driving, gameTime);
				}
			}
			else if (State == CarState.MovingLeft)
			{
				if (Lane != PreviousLane)
				{
					ChangeState(CarState.Driving, gameTime);
				}
			}

			Body b = AnticipateCollision(3);

			if (b != null)
			{
				Velocity = new Vector2(Velocity.X, Math.Max(0, Math.Min(Velocity.Y, b.LinearVelocity.Y - 1)));
				if (Velocity.Y <= 0.1f)
				{
					Velocity = new Vector2(Velocity.X, 0);
				}
			}
			else
			{
				Velocity = new Vector2(Velocity.X, InitialSpeed);
			}

			if (Math.Abs(Velocity.Y - DesiredSpeed) > 0.1)
			{
				Body.LinearVelocity = new Vector2(Velocity.X, Velocity.Y * 0.9f + DesiredSpeed * 0.1f);
			}

			if (DesiredLane != Lane)
			{
				float maxLateralSpeed = 2.0f;
				float lateralSpeed = Velocity.X * 0.5f + maxLateralSpeed * (DesiredLane - Lane) * 0.5f;

				int actualLane = Road.GetLane(Position.X);

				if (actualLane == DesiredLane)
				{
					lateralSpeed = 0;
					Lane = DesiredLane;
				}

				Velocity = new Vector2(lateralSpeed, Velocity.Y);
			}

			float desiredAngle = -Velocity.X * 0.05f;
			Body.Rotation = Body.Rotation * 0.5f + desiredAngle * 0.5f;
		}

		public void Render(SpriteBatch spriteBatch, GameTime gameTime, Matrix projection, Matrix view, Matrix lightViewProjection, Texture2D shadowMap, string technique)
		{
			model.Position = new Vector3(Position, 0);
			model.Size = new Vector3(Type.Width, 1.5f, Type.Height);
			model.Rotation = Matrix.CreateFromAxisAngle(Vector3.Left, -MathHelper.PiOver2) * Matrix.CreateFromAxisAngle(Vector3.Backward, Body.Rotation);// new Vector3(0, 0, Body.Rotation);

			model.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
		}


		public static void LoadContent(ContentManager content)
		{
			CarType.LoadContent(content);
			//model = new RenderedModel(content, c)
		}
	}
}
