using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FitMiTraffic.Main.Vehicle
{
	class CarType
	{


		public static readonly CarType RED_CAR = new CarType("red_car", 1.74f, 3.57f, 8, 2, 14, 0);
		public static readonly CarType BLUE_CAR = new CarType("blue_car", 1.74f, 3.57f, 8, 2, 14, 0);
		public static readonly CarType PORSCHE = new CarType("porsche", 1.6f, 3.79f, 5, 1, 59, 72);
		public static readonly CarType SEMI_TRUCK = new CarType("semi_truck", 1.8f, 6.4f, 0, 0, 0, 0);
		public static readonly CarType SEDAN = new CarType("sedan", 1.5f, 3.11f, 7, 1, 19, 20);
		public static readonly CarType MERCEDES = new CarType("mercedes", 1.6f, 3.56f, 25, 20, 90, 65);

		public string TextureName;
		public float Width;
		public float Height;
		public int OffsetX; //Number of empty pixels on left or right
		public int OffsetY; //Number of empty pixels on top or bottom
		public float RadiusTop;
		public float RadiusBottom;
		public Texture2D Texture;


		public CarType(string textureName, float width, float height, int offsetX, int offsetY, float radiusTop, float radiusBottom)
		{
			TextureName = textureName;
			Width = width;
			Height = height;
			OffsetX = offsetX;
			OffsetY = offsetY;
			RadiusTop = radiusTop;
			RadiusBottom = radiusBottom;
		}

		public static CarType RANDOM
		{
			get
			{
				Random random = new Random();
				int v = random.Next(0, 6);

				switch (v)
				{
					case 0: return RED_CAR;
					case 1: return BLUE_CAR;
					case 2: return PORSCHE;
					case 3: return SEMI_TRUCK;
					case 4: return SEDAN;
				}

				return RED_CAR;
			}
		}

		public static CarType[] ALL
		{
			get
			{
				return new CarType[]
				{
					MERCEDES,
					RED_CAR,
					BLUE_CAR,
					PORSCHE,
					SEMI_TRUCK,
					SEDAN
				};
			}
		}

		public static void LoadContent(ContentManager content)
		{
			foreach (CarType ct in CarType.ALL)
			{
				ct.Texture = content.Load<Texture2D>(ct.TextureName);
			}
		}
	}
}
