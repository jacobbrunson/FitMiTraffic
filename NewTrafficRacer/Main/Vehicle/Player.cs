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

namespace NewTrafficRacer.Vehicle
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
		int recenterDir = 1;

		float previousTime;

		Body ApproachingBody;

		public delegate void DodgeCompleteDelegate(Body b);
		public DodgeCompleteDelegate DodgeCompleteCallback;

        public delegate void CoinGetDelegate(Body b);
        public CoinGetDelegate CoinGetCallback;

		Queue<Dodge> Dodges = new Queue<Dodge>();

        float initialSpeed;

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
			Reset();

			//var s = content.Load<SoundEffect>("Buzz2");
			//sound = s.CreateInstance();
			//sound.IsLooped = true;
			//sound.Play();
			model.Color = new Color(229, 189, 15, 255);

		}

		public void Reset()
		{
			crashed = false;
			Position = Vector2.Zero;
			Body.Rotation = 0;
			Body.LinearDamping = 0.0f;
			Body.LinearVelocity = new Vector2(0, initialSpeed);
			Body.SetFriction(0);
		}

		public void Update(GameTime gameTime, float movement)
		{
			if (crashed)
			{
				float dt = previousTime - crashTime;
				float damping = dt * dt * 4;
				Body.LinearDamping = damping;
				Body.AngularDamping = damping;
			} else
			{
				float maxLateralSpeed = 6.5f;

				float desiredSpeed = movement * 0.5f * maxLateralSpeed;

				float lateralSpeed = desiredSpeed * 0.5f + Body.LinearVelocity.X * 0.5f;

				Body.LinearVelocity = new Vector2(lateralSpeed, Body.LinearVelocity.Y * 0.98f + initialSpeed * 0.02f);
				Body.Rotation = -lateralSpeed * 0.05f;


                //Dodge code
				Body b = AnticipateCollision(4.0f);

				if (ApproachingBody != null && !ApproachingBody.Equals(b))
				{
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
						DodgeCompleteCallback(d.Body);
						Dodges.Dequeue();
					}
				}
			}

			previousTime = (float)gameTime.TotalGameTime.TotalSeconds;
		}

		public void Recenter(GameTime gameTime)
		{
			recenterTime = gameTime.TotalGameTime.TotalSeconds;
			if (Position.X > 0 && Body.Rotation < 0 && Body.Rotation > -MathHelper.PiOver2 || Position.X <= 0 && Body.Rotation > 0 && Body.Rotation < MathHelper.PiOver2)
			{
				recenterDir = -1;
			}
			else
			{
				recenterDir = 1;
			}
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

            if (b.Body.BodyType == BodyType.Kinematic)
            {
                CoinGetCallback(b.Body);
                return true;
            }

			crashed = true;
			crashTime = previousTime;
			return true;
		}
	}
}
