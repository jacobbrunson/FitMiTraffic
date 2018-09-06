using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Graphics
{
	class PostProcessor
	{
		private SpriteBatch spriteBatch;
		private GraphicsDevice graphics;
		private RenderTarget2D target;
		private Effect effect;

		public PostProcessor(GraphicsDevice graphics, SpriteBatch spriteBatch, Effect effect)
		{
			this.graphics = graphics;
			this.spriteBatch = spriteBatch;
			this.effect = effect;
			target = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
		}

		public void Begin()
		{
			graphics.SetRenderTarget(target);
		}

		public void End(bool apply=true)
		{
			graphics.SetRenderTarget(null);
			if (apply)
				spriteBatch.Begin(effect: effect);
			else
				spriteBatch.Begin();
			spriteBatch.Draw(target, Vector2.Zero, Color.White);
			spriteBatch.End();
		}

	}
}
