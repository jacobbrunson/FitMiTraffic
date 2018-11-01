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
		private int highlightedLane;
		private Highlight highlight;

        private World world;

		public RoadSegment(ContentManager content, World world, float y, int highlightedLane, int groundWidth=10, int groundOffsetX=0, float biomeScale=100)
		{
			
			ground = new Ground(content, y, groundWidth, groundOffsetX, biomeScale);
			road = new RoadPiece(content, y);
			leftRail = new Rail(content, world, -Road.Size/2, y);
			rightRail = new Rail(content, world, Road.Size/2, y);
			Y = y;
			this.highlightedLane = highlightedLane;
			highlight = new Highlight(content);
			this.world = world;
		}

		public void Render(GameTime gameTime, GraphicsDevice graphics, BaseEffect effect)
		{
			ground.positionY = Y;
			road.Position = new Vector3(road.Position.X, Y, road.Position.Z);
			leftRail.Position = new Vector3(leftRail.Position.X, Y, leftRail.Position.Z);
			rightRail.Position = new Vector3(rightRail.Position.X, Y, rightRail.Position.Z);
			if (effect.Technique.Name.Equals("ShadowedScene"))
			{
				//ground.Render(graphics, effect);
			}

			if (highlightedLane >= 0)
			{
				var X = (highlightedLane - Road.NumLanes/2 + 1) * Road.LaneWidth * 0.95f;
				highlight.Position = new Vector3(X, Y, 0);
				highlight.Render(gameTime, effect);
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
