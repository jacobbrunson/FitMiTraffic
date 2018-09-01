using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using FitMiTraffic.Main.Utility;

namespace FitMiTraffic.Main.Vehicle
{
	class Dodge
	{
		public Body Body;
		public double Time;

		public Dodge(Body body, double time)
		{
			Body = body;
			Time = time;
		}
	}

	class Player : Car
	{
		public bool crashed = false;
		float crashTime;

		double recenterTime = -1;

		float previousTime;

		Body ApproachingBody;

		public delegate void DodgeCompleteDelegate();
		public DodgeCompleteDelegate DodgeCompleteCallback;

		Queue<Dodge> Dodges = new Queue<Dodge>();

        private float initialSpeed;

		SoundEffectInstance sound;

		public Vector2 Velocity
		{
			get { return Body.LinearVelocity; }
			set
			{
				Body.LinearVelocity = new Vector2(value.X, Body.LinearVelocity.Y);
				initialSpeed = value.Y;
			}
		}

		public Player(ContentManager content, CarType type, World world, float initialSpeed) : base(content, type, world, initialSpeed)
		{
            this.initialSpeed = initialSpeed;
			Body.OnCollision += Collision;
			Body.Mass = 4000.0f;
			Body.LinearDamping = 0.0f;
			Body.LinearVelocity = new Vector2(0, initialSpeed);
            Body.SetFriction(0);

			var s = content.Load<SoundEffect>("loop_0");
			sound = s.CreateInstance();
			sound.IsLooped = true;
			//sound.Play();

		}

		public void Update(GameTime gameTime, float movement)
		{
			if (crashed)
			{
				float dt = previousTime - crashTime;
				float damping = dt * dt * 4;
				Body.LinearDamping = damping;
				Body.AngularDamping = damping;

				if (recenterTime > 0)
				{
					Body.LinearDamping = 0;
					Body.AngularDamping = 0;

					
					float speed = 0;
					float targetAngle;
					if (Math.Abs(Body.Position.X) > 1f)
					{
						speed = Math.Abs(Body.Position.X);
						targetAngle = MathHelper.PiOver2 * (Position.X < 0 ? -1 : 1);

						if (Math.Abs(Body.Position.X) > 2f && Math.Abs(Body.Rotation - targetAngle) > MathHelper.PiOver2)
						{
							speed = -1;
							targetAngle *= -1;
						}
					} else
					{
						speed = Math.Abs(Body.Rotation).Map(MathHelper.PiOver2, 0, 0, 5);
						targetAngle = 0;
					}
					Body.Rotation = Body.Rotation * 0.95f + targetAngle * 0.05f;
					Body.LinearVelocity = new Vector2((float)-Math.Sin(Body.Rotation) * speed, (float)Math.Cos(Body.Rotation) * speed);
					
					if (targetAngle == 0 && Math.Abs(Body.Rotation) < 0.1f)
					{
						crashed = false;
						recenterTime = -1;
					}
				}
			} else
			{
				float maxLateralSpeed = 6.5f;

				float desiredSpeed = movement * 0.5f * maxLateralSpeed;

				float lateralSpeed = desiredSpeed * 0.5f + Body.LinearVelocity.X * 0.5f;

				Body.LinearVelocity = new Vector2(lateralSpeed, Body.LinearVelocity.Y * 0.98f + initialSpeed * 0.02f);
				Body.Rotation = -lateralSpeed * 0.05f;

				Body b = AnticipateCollision(2.0f);

				if (ApproachingBody != null && !ApproachingBody.Equals(b))
				{
					//Game1.DodgeCompleted();
					Dodges.Enqueue(new Dodge(ApproachingBody, gameTime.TotalGameTime.TotalSeconds));
					ApproachingBody = null;
				}

				if (b != null)
				{
					ApproachingBody = b;
				}

				if (Dodges.Count > 0)
				{
					Dodge d = Dodges.Peek();
					AABB aabb;
					d.Body.FixtureList[0].GetAABB(out aabb, 0);
					if (d.Body.Position.Y - aabb.Height / 2 < Position.Y)
					{
						DodgeCompleteCallback();
						Dodges.Dequeue();
					}
				}
			}

			previousTime = (float)gameTime.TotalGameTime.TotalSeconds;
		}

		public void Recenter(GameTime gameTime)
		{
			recenterTime = gameTime.TotalGameTime.TotalSeconds;
		}

		public Boolean IsRecentering()
		{
			return recenterTime > 0;
		}

		public static void LoadContent()
		{

		}

		bool Collision(Fixture a, Fixture b, Contact c)
		{
            if (b.Body.BodyType == BodyType.Static)
            {
                
                return true;
            }

			crashed = true;
			crashTime = previousTime;
			//b.Body.ApplyLinearImpulse(new Vector2(-100, 20));
			return true;
		}
	}
}
