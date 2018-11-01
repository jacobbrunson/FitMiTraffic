using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FitMiTraffic.Main.Utility;
using FitMiTraffic.Main.Vehicle;
using tainicom.Aether.Physics2D.Dynamics;
using FitMiTraffic.Main.Graphics;

namespace FitMiTraffic.Main.Environment
{
	class Road
	{
		LinkedList<RoadSegment> Segments = new LinkedList<RoadSegment>();

		public const int NumLanes = 4;
		public const float LaneWidth = 2.45f;
		public const float Size = NumLanes * LaneWidth;

		public int Highlight = 0;

		public static Vector2 Scale;
		//private static Texture2D Texture;
		private static Vector2 TextureSize;

		private static ContentManager content;

        private World world;

		public bool CullBack = true;
		private int groundWidth = 10;
		private int groundOffsetX = 0;
        private float biomeScale = 100;

		public Road(World world, int groundWidth = 10, int groundOffsetX = 0, float biomeScale = 100)
		{
            this.world = world;
			this.groundWidth = groundWidth;
			this.groundOffsetX = groundOffsetX;
            this.biomeScale = biomeScale;
			Reset();
		}

		public void Reset()
		{
			Segments.Clear();
			if (CullBack)
			{
				for (int i = 0; i < 10; i++)
				{
					var piece = new RoadSegment(content, world, Size * (i - 1), Highlight);
					Segments.AddLast(piece);
				}
			} else
			{
				for (int i = 0; i < 15; i++)
				{
					var piece = new RoadSegment(content, world, Size * (-i), Highlight, groundWidth, groundOffsetX, biomeScale);
					Segments.AddLast(piece);
				}
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
			if (CullBack && playerY - Segments.First.Value.Y > Size*2)
			{
                Segments.First.Value.Destroy();
				Segments.RemoveFirst();
				var piece = new RoadSegment(content, world, Segments.Last.Value.Y + Size, Highlight);
				Segments.AddLast(piece);
			} else if (!CullBack && Segments.First.Value.Y < playerY)
			{
				Segments.Last.Value.Destroy();
				Segments.RemoveLast();
				var piece = new RoadSegment(content, world, Segments.First.Value.Y + Size, Highlight, groundWidth, groundOffsetX);
				Segments.AddFirst(piece);
			}
		}

		public void Render(GameTime gameTime, GraphicsDevice graphics, BaseEffect effect)
		{
			foreach (RoadSegment p in Segments)
			{
				p.Render(gameTime, graphics, effect);
			}
		}

		public static void LoadContent(ContentManager content)
		{
			Road.content = content;
			//Texture = content.Load<Texture2D>("road");
			//TextureSize = new Vector2(Texture.Width, Texture.Height);
			Scale = Vector2.One * Size / TextureSize;
		}

	}
}
