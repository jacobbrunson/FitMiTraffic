using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FitMiTraffic.Main.Environment
{
	class Road
	{
		LinkedList<float> segments = new LinkedList<float>();

		public const int NumLanes = 4;
		public const float LaneWidth = 2.25f;
		private const float Size = NumLanes * LaneWidth;

		private Texture2D texture;
		private Vector2 textureSize;

		public Road(Texture2D texture)
		{
			this.texture = texture;
			this.textureSize = new Vector2(texture.Width, texture.Height);
			for (int i = 0; i < 6; i++)
			{
				segments.AddLast(Size * (i-1));
			}
		}

		public void Update(float playerY)
		{
			if (playerY - segments.First.Value > Size*2)
			{
				segments.RemoveFirst();
				segments.AddLast(segments.Last.Value + Size);
			}
		}

		public void Render(SpriteBatch spriteBatch)
		{
			foreach (float p in segments)
			{
				spriteBatch.Draw(texture, new Vector2(0, p), null, Color.White, 0,
					textureSize / 2, Vector2.One * Size / textureSize, SpriteEffects.None, 0.0f);
			}
		}

	}
}
