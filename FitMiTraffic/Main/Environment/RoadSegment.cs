using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace FitMiTraffic.Main.Environment
{
	class RoadSegment
	{
		public float Y;

		private Ground ground;
		private RoadPiece road;
		private Rail leftRail;
		private Rail rightRail;

        private World world;

		public RoadSegment(ContentManager content, World world, float y)
		{
			Y = y;
			ground = new Ground(content, y);
			road = new RoadPiece(content, y);
			leftRail = new Rail(content, world, -Road.Size/2, y);
			rightRail = new Rail(content, world, Road.Size/2, y);
            this.world = world;
		}

		public void Render(GraphicsDevice graphics, GameTime gameTime, Matrix view, Matrix projection, Matrix lightViewProjection, Texture2D shadowMap, string technique)
		{
			ground.Render(graphics, view, projection);
			road.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
			leftRail.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
			rightRail.Render(gameTime, view, projection, lightViewProjection, shadowMap, technique);
		}

        public void Destroy()
        {
            leftRail.Destroy();
            rightRail.Destroy();
        }

	}
}
