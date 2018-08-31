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

namespace FitMiTraffic.Main.Environment
{
	class Road
	{
		LinkedList<RoadSegment> Segments = new LinkedList<RoadSegment>();

		public const int NumLanes = 4;
		public static float LaneWidth = 2.45f;
		public static float Size = NumLanes * LaneWidth;

		public static Vector2 Scale;
		//private static Texture2D Texture;
		private static Vector2 TextureSize;

		private static ContentManager content;

		Car car;

		public Road()
		{
			Console.WriteLine("HEY");
			for (int i = 0; i < 10; i++)
			{
				var piece = new RoadSegment(content, Size * (i - 1));
				Segments.AddLast(piece);
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
			if (playerY - Segments.First.Value.Y > Size*2)
			{
				Segments.RemoveFirst();
				var piece = new RoadSegment(content, Segments.Last.Value.Y + Size);
				Segments.AddLast(piece);
			}
		}

		public void Render(GameTime gameTime, Matrix view, Matrix projection, Matrix lightViewProjection, Texture2D shadowMap, string technique)
		{
			foreach (RoadSegment p in Segments)
			{
				p.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
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
