﻿using Microsoft.Xna.Framework;
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
	public class SpeedLimit : RenderedModel
	{
		private const string ModelName = "SpeedSign";
		public SpeedLimit(ContentManager content) : base(content, ModelName)
		{
			//this.Rotation = new Vector3(0, 0, -MathHelper.PiOver2);
		}
	}
}