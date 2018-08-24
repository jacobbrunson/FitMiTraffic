using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;

namespace TestGame
{
	class Player : Car
	{
		public bool crashed = false;
		float crashTime;

		float previousTime;

		public Player(CarType type, World world, Texture2D texture, float initialSpeed) : base(type, world, texture, initialSpeed)
		{
			Console.WriteLine(Body.Mass);
			Body.OnCollision += Collision;
			Body.Mass = 4000.0f;
		}

		public void Update(GameTime gameTime, KeyboardState keyboardState, int gyro)
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

				float desiredSpeed = 0;

				if (gyro < -50)
				{
					desiredSpeed = Math.Max(-maxLateralSpeed, gyro * 0.005f);
				} else if (gyro > 50)
				{
					desiredSpeed = Math.Min(maxLateralSpeed, gyro * 0.005f);
				} else
				{
					if (keyboardState.IsKeyDown(Keys.A))
					{
						desiredSpeed = -maxLateralSpeed / 2;
					}
					else if (keyboardState.IsKeyDown(Keys.D))
					{
						desiredSpeed = maxLateralSpeed / 2;
					}
				}

				float lateralSpeed = desiredSpeed * 0.5f + Body.LinearVelocity.X * 0.5f;

				Body.LinearVelocity = new Vector2(lateralSpeed, 15.0f);
				Body.Rotation = -lateralSpeed * 0.1f;
			}

			previousTime = (float)gameTime.TotalGameTime.TotalSeconds;
		}

		bool Collision(Fixture a, Fixture b, Contact c)
		{
			crashed = true;
			crashTime = previousTime;
			//b.Body.ApplyLinearImpulse(new Vector2(-100, 20));
			return true;
		}
	}
}
