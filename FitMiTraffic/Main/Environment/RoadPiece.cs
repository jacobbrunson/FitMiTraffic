using FitMiTraffic.Main.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Environment
{
	public class RoadPiece : RenderedModel
	{
		private const string ModelName = "road";
		public RoadPiece(ContentManager content, float y) : base(content, ModelName)
		{
			this.Position = new Vector3(0, y, 0);
			this.Rotation = Matrix.CreateFromYawPitchRoll(0, -MathHelper.Pi, 0);
			this.Size = new Vector3(Road.Size, Road.Size, 0.1f);
		}
	}
}
