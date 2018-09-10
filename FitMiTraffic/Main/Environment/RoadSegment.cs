using FitMiTraffic.Main.Graphics;
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

		public void Render(GameTime gameTime, GraphicsDevice graphics, BaseEffect effect)
		{
			if (effect.Technique.Name.Equals("ShadowedScene"))
			{
				ground.Render(graphics, effect);
			}
			road.Render(gameTime, effect);
			leftRail.Render(gameTime, effect);
			rightRail.Render(gameTime, effect);
		}

        public void Destroy()
        {
            leftRail.Destroy();
            rightRail.Destroy();
        }

	}
}
