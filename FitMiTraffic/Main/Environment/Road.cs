using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FitMiTraffic.Main.Environment
{
	class Road
	{
		LinkedList<float> segments = new LinkedList<float>();

		public const int NumLanes = 4;
		public const float LaneWidth = 2.25f;
		private const float Size = NumLanes * LaneWidth;

		private static Texture2D Texture;
		private Vector2 textureSize;

		public Road()
		{
			this.textureSize = new Vector2(Texture.Width, Texture.Height);
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
				spriteBatch.Draw(Texture, new Vector2(0, p), null, Color.White, 0,
					textureSize / 2, Vector2.One * Size / textureSize, SpriteEffects.None, 0.0f);
			}
		}

		public static void LoadContent(ContentManager content)
		{
			Texture = content.Load<Texture2D>("road");
		}

	}
}
