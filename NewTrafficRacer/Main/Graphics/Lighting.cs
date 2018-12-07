using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Graphics
{
	public class Lighting
	{
		private const float nearZ = -10;
		private const float farZ = 40;
		private const float width = 25;
		private const float height = 50;

		public Vector3 Position;
		public Vector3 Direction;

		public Matrix View;
		public Matrix Projection;
		public RenderTarget2D ShadowMap;


		public Lighting(GraphicsDevice graphics, int shadowRes=2048)
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
