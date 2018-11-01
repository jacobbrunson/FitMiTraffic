using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FitMiTraffic.Main.Graphics;

namespace FitMiTraffic.Main.Environment
{
	public class BigSign : RenderedModel
	{
		private const string ModelName = "bigsign";

		public BigSign(ContentManager content) : base(content, ModelName)
		{
			//this.Rotation = Matrix.Identity
			this.Offset = new Vector3(-6, 0, 0);
			this.Size = new Vector3(12, 1, 6);
		}
	}
}
