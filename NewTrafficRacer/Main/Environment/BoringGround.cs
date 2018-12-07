using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NewTrafficRacer.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NewTrafficRacer.Environment
{
	public class BoringGround : RenderedModel
	{
		private const string ModelName = "ground";

		public BoringGround(ContentManager content) : base(content, ModelName)
		{
			this.Offset = new Vector3(0, 0, -0.5f);
			this.Size = new Vector3(50, 10, 0.5f);
		}
	}
}
