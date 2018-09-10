using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Graphics
{
	enum CameraMode
	{
		PERSPECTIVE, FLAT
	}
	class Camera
	{
		private const float orthoNearZ = -1000;
		private const float orthoFarZ = 1000;
		private const float perspNearZ = 0.1f;
		private const float perspFarZ = 100;
		private const float fov = MathHelper.PiOver2 * 0.8f;

		public Vector2 Target;
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
			if (Mode == CameraMode.PERSPECTIVE)
			{
				position = target + new Vector3(0, -5 * Zoom, 12 * Zoom);
				Projection = Matrix.CreatePerspectiveFieldOfView(fov, (float)Width / (float)Height, perspNearZ, perspFarZ);
			}
			else
			{
				position = target + new Vector3(0, 0, 10);
				Projection = Matrix.CreateOrthographic(Width / Scale, Height / Scale, orthoNearZ, orthoFarZ);
			}
			position = Vector3.Transform(position - target, Matrix.CreateFromYawPitchRoll(Revolution.X, Revolution.Y, 0)) + target;

			View = Matrix.CreateLookAt(position, target, Vector3.Up);
		}
	}
}
