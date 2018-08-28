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
		public static readonly CarType TEST = new CarType("test", 1.6f, 3.9f, 0, 0, 0.04f);

		public string Name;
		public float Width;
		public float Height;
		public float RadiusTop;
		public float RadiusBottom;
		public Model Model;
		public Texture2D Texture;
		public float Scale;


		public CarType(string name, float width, float height, float radiusTop, float radiusBottom, float modelScale)
		{
			Name = name;
			Width = width;
			Height = height;
			RadiusTop = radiusTop;
			RadiusBottom = radiusBottom;
			Scale = modelScale;
		}

		public static CarType RANDOM
		{
			get
			{
				return CarType.TEST;
				Random random = new Random();
				int i = random.Next(1, CarType.ALL.Length);

				return CarType.ALL[i];
			}
		}

		public static CarType[] ALL
		{
			get
			{
				return new CarType[]
				{
					TEST
				};
			}
		}

		public static void LoadContent(ContentManager content)
		{
			foreach (CarType ct in CarType.ALL)
			{
				ct.Texture = content.Load<Texture2D>("cars/tex/" + ct.Name);
				ct.Model = content.Load<Model>("cars/" + ct.Name);
			}
		}
	}
}
