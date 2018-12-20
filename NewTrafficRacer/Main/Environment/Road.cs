using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NewTrafficRacer.Utility;
using tainicom.Aether.Physics2D.Dynamics;

namespace NewTrafficRacer.Environment
{
    class Road
    {
        //Parameters
        public const int NumLanes = 4;
        public const float LaneWidth = 2.45f;
        public const float Size = NumLanes * LaneWidth;
        const int highlightChangeInterval = 10;
        
        //State
        LinkedList<RoadSegment> Segments = new LinkedList<RoadSegment>();
        public int Highlight = -1;
        public double HighlightChangeTime = -10;

        ContentManager content;
        World world;


        public Road(ContentManager content, World world)
        {
            this.content = content;
            this.world = world;
            Reset();
        }

        //Destroy the road and initially generate 10 pieces at the beginning
        public void Reset()
        {
            Segments.Clear();
            for (int i = 0; i < 10; i++)
            {
                var piece = new RoadSegment(content, world, Size * (i - 1), Highlight);
                Segments.AddLast(piece);
            }
        }

        //Get the lane whose center is at a given x-coordinate
        public static int GetLane(float x, float tolerance = 0.1f)
        {
            float f = x.Map(-LaneWidth * NumLanes / 2, LaneWidth * NumLanes / 2, 0, NumLanes);
            int i = (int)f;

            if (Math.Abs(x - GetCenterOfLane(i)) > tolerance)
            {
                return -100; //This value must be < 0 but must not == -1. Ugly hack.
            }

            return i;
        }

        //Used to make a given lane have gold arrows
        public void SetHighlightStatus(int lane)
        {
            foreach (RoadSegment segment in Segments)
            {
                segment.SetHighlightStatus(lane);
            }
        }

        //Returns a world-space X coordinate of a given lane center
        public static float GetCenterOfLane(int lane)
        {
            return LaneWidth * (lane - NumLanes / 2) + LaneWidth / 2;
        }

        //Get which lane is highlighted at the player-end of the road
        //(segments further ahead of the player may have a different highlighted lane)
        public int GetHighlightAtPlayerPos()
        {
            return Segments.First.Next.Value.HighlightedLane;
        }

        public void Update(GameTime gameTime, float playerY)
        {
            //Destroy road segments which the player has passed,
            //and create a new one far ahead of the player
            if (playerY - Segments.First.Value.Y > Size * 2)
            {
                Segments.First.Value.Destroy();
                Segments.RemoveFirst();
                var piece = new RoadSegment(content, world, Segments.Last.Value.Y + Size, Highlight);
                Segments.AddLast(piece);
            }

            //Change the highlated lane
            double d = gameTime.TotalGameTime.TotalSeconds - HighlightChangeTime;
            if (d > highlightChangeInterval)
            {
                HighlightChangeTime = gameTime.TotalGameTime.TotalSeconds;
                Highlight = new Random().Next(4);
            }
        }

        public void Render(GameTime gameTime, GraphicsDevice graphics, Effect effect)
        {
            foreach (RoadSegment p in Segments)
            {
                p.Render(gameTime, graphics, effect);
            }
        }
    }
}

