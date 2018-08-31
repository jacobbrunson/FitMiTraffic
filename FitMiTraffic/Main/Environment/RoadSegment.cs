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
	class RoadSegment
	{
		public float Y;

		private RoadPiece road;
		private Rail leftRail;
		private Rail rightRail;

		public RoadSegment(ContentManager content, float y)
		{
			Y = y;
			road = new RoadPiece(content, y);
			leftRail = new Rail(content, -Road.Size/2, y);
			rightRail = new Rail(content, Road.Size/2, y);
		}

		public void Render(GameTime gameTime, Matrix view, Matrix projection, Matrix lightViewProjection, Texture2D shadowMap, string technique)
		{
			road.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
			leftRail.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
			rightRail.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
		}

	}
}
