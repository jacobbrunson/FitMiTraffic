using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using NewTrafficRacer.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Environment
{
	public class RoadPiece : RenderedModel
	{
		const string ModelName = "road_model";
		public RoadPiece(ContentManager content, float y) : base(content, ModelName, "road")
		{
			this.Scale = new Vector3(1, 1.05f, 1);
			this.Position = new Vector3(0, y, 0);
			this.Rotation = Matrix.CreateFromYawPitchRoll(0, -MathHelper.Pi, 0);
			this.Size = new Vector3(Road.Size, Road.Size, 0.1f);
		}
	}
}
