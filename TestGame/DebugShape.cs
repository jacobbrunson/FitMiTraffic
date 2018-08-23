using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestGame
{
	class DebugShape
	{
		public static Texture2D TextureRectangle;
		public static Texture2D TextureCircle;

		public static void RenderRectangle(SpriteBatch spriteBatch, Vector2 position, float width, float height, float rotation)
		{
			var Texture = TextureRectangle;
			var TextureSize = new Vector2(Texture.Width, Texture.Height);
			var Scale = new Vector2(width, height) / TextureSize;
			spriteBatch.Draw(Texture, position, null, Color.White, rotation,
				TextureSize / 2, Scale, SpriteEffects.FlipVertically, 0.0f);
		}

		public static void RenderEllipse(SpriteBatch spriteBatch, Vector2 position, float width, float height, float rotation)
		{
			var Texture = TextureCircle;
			var TextureSize = new Vector2(Texture.Width, Texture.Height);
			var Scale = new Vector2(width, height) / TextureSize;
			spriteBatch.Draw(Texture, position, null, Color.White, rotation,
				TextureSize / 2, Scale, SpriteEffects.FlipVertically, 0.0f);
		}
	}
}
