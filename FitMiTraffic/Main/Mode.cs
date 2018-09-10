using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main
{
	public abstract class Mode
	{

		public abstract void Update(GameTime gameTime);
		public abstract void Render(GameTime gameTime);

	}
}
