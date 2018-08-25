using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FitMiTraffic.Main.Utility;

namespace FitMiTraffic.Main.Environment
{
	class Road
	{
		LinkedList<float> Segments = new LinkedList<float>();

		public const int NumLanes = 4;
		public static float LaneWidth = 2.25f;
		private float Size = NumLanes * LaneWidth;

		private static Texture2D Texture;
		private static Vector2 TextureSize;

		public Road()
		{
			for (int i = 0; i < 6; i++)
			{
				Segments.AddLast(Size * (i-1));
			}
		}

		public static int GetLane(float x, float tolerance=0.1f)
		{
			float f = x.Map(-LaneWidth * NumLanes / 2, LaneWidth * NumLanes / 2, 0, NumLanes);
			int i = (int)f;

			if (Math.Abs(x - GetCenterOfLane(i)) > tolerance)
			{
				return -1;
			}

			return i;
		}

		public static float GetCenterOfLane(int lane)
		{
			return LaneWidth * (lane - NumLanes / 2) + LaneWidth / 2;
		}

		public void Update(float playerY)
		{
			if (playerY - Segments.First.Value > Size*2)
			{
				Segments.RemoveFirst();
				Segments.AddLast(Segments.Last.Value + Size);
			}
		}

		public void Render(SpriteBatch spriteBatch)
		{
			foreach (float p in Segments)
			{
				spriteBatch.Draw(Texture, new Vector2(0, p), null, Color.White, 0,
					TextureSize / 2, Vector2.One * Size / TextureSize, SpriteEffects.None, 0.0f);
			}
		}

		public static void LoadContent(ContentManager content)
		{
			Texture = content.Load<Texture2D>("road");
			TextureSize = new Vector2(Texture.Width, Texture.Height);
		}

	}
}
