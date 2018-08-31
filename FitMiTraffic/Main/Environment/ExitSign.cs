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
	public class ExitSign : RenderedModel
	{
		private const string ModelName = "Exit";

		public ExitSign(ContentManager content) : base(content, ModelName)
		{
			//this.Rotation = new Vector3(0, 0, -MathHelper.PiOver2);
		}
	}
}
