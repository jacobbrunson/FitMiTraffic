using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitMiTraffic.Main.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FitMiTraffic.Main.Environment
{
	public class Ground : RenderedModel
	{
		private const string ModelName = "ground";

		public Ground(ContentManager content) : base(content, ModelName)
		{
			//this.Rotation = Matrix.Identity
			this.Offset = new Vector3(0, 0, 0);
			this.Size = new Vector3(50, 50, 0.1f);
		}
	}
}