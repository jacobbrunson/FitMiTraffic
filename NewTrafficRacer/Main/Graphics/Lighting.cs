using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Graphics
{
	public static class Lighting
	{
		const float nearZ = -10;
		const float farZ = 40;
		const float width = 25;
		const float height = 50;
        const int shadowRes = 2048;

		public static Vector3 Position;
		public static Vector3 Direction;

		public static Matrix View;
		public static Matrix Projection;
		public static RenderTarget2D ShadowMap;

		public static void Initialize()
		{
			Projection = Matrix.CreateOrthographic(width, height, nearZ, farZ);
			ShadowMap = new RenderTarget2D(TrafficGame.Graphics, shadowRes, shadowRes, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
		}

		public static void Update()
		{
			View = Matrix.CreateLookAt(Position, Position + Direction, Vector3.Up);
		}
	}
}
