﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Graphics
{
	class Lighting
	{

		private const float width = 25;
		private const float height = 50;
		private const float nearZ = 10;
		private const float farZ = 40;
		private const int shadowRes = 2048;

		public Vector3 Position;
		public Vector3 Direction;

		public Matrix View;
		public Matrix Projection;
		public RenderTarget2D ShadowMap;


		public Lighting(GraphicsDevice graphics)
		{
			Projection = Matrix.CreateOrthographic(width, height, nearZ, farZ);
			ShadowMap = new RenderTarget2D(graphics, shadowRes, shadowRes, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
		}

		public void Update()
		{
			View = Matrix.CreateLookAt(Position, Position + Direction, Vector3.Up);
		}
	}
}