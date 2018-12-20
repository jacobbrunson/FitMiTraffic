﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Graphics
{
	public enum CameraMode
	{
		PERSPECTIVE, FLAT, FIXED
	}
	public class Camera
	{
		const float orthoNearZ = -1000;
		const float orthoFarZ = 1000;
		const float perspNearZ = 0.1f;
		const float perspFarZ = 100;
		const float fov = MathHelper.PiOver2 * 0.8f;

		public Vector2 Target;
		public Vector2 Offset = Vector2.Zero;
		public Vector2 Revolution;

		public Matrix View;
		public Matrix Projection;

		public int Width;
		public int Height;
		public CameraMode Mode = CameraMode.PERSPECTIVE;
		public float Zoom = 1;
		public float Scale = 40;

		public Camera(int width, int height)
		{
			Width = width;
			Height = height;
		}

		public void Update()
		{
			Vector3 position;
			Vector3 target = new Vector3(0 * Zoom + Target.X * (1 - Zoom), Target.Y + 5 * Zoom, 0);
			Vector3 up = Vector3.Up;
			if (Mode == CameraMode.PERSPECTIVE)
			{
				position = target + new Vector3(0, -6 * Zoom, 11 * Zoom);
				Projection = Matrix.CreatePerspectiveFieldOfView(fov, (float)Width / (float)Height, perspNearZ, perspFarZ);
			}
			else if (Mode == CameraMode.FLAT)
			{
				position = target + new Vector3(0, 0, 10);
				Projection = Matrix.CreateOrthographic(Width / Scale, Height / Scale, orthoNearZ, orthoFarZ);
			}
			else
			{
				target = new Vector3(Target, 0);
				position = new Vector3(Offset + Target, 2);
				up = Vector3.UnitZ;
				Projection = Matrix.CreatePerspectiveFieldOfView(fov, (float)Width / (float)Height, perspNearZ, perspFarZ);
			}
			position = Vector3.Transform(position - target, Matrix.CreateFromYawPitchRoll(Revolution.X, Revolution.Y, 0)) + target;

			View = Matrix.CreateLookAt(position, target, up);

			if (Mode == CameraMode.FIXED)
			{
				View *= Matrix.CreateTranslation(Vector3.UnitY * -1);
			}
		}
	}
}
