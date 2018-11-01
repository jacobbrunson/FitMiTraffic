using FitMiTraffic.Main.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Environment
{
	class Highlight : RenderedModel
	{
		private const string ModelName = "highlight";

		public Highlight(ContentManager content) : base(content, ModelName)
		{
			//this.Rotation = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, MathHelper.PiOver2, 0);
			this.Size = new Vector3(Road.LaneWidth * 0.8f, Road.LaneWidth * 0.8f, 0.05f);
			this.Offset = new Vector3(-Road.LaneWidth/2 + 0.05f, 0, 0.05f);
			this.Rotation = Matrix.CreateFromYawPitchRoll(0, 0.05f, 0);
		}

		public void Update(GameTime gameTime)
		{
			//this.Rotation *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, (float)gameTime.ElapsedGameTime.TotalSeconds);
		}
	}
}
