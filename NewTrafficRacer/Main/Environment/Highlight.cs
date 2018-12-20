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
	class Highlight : RenderedModel
	{
		const string modelName = "highlight";
        const string texName = "HighlightTex";

        int lane = -1;

        public int HighlightOn = -100;

		public Highlight(ContentManager content, int lane) : base(content, modelName, texName)
		{
            this.Size = new Vector3(Road.LaneWidth * 0.8f, Road.LaneWidth * 0.8f, 0.05f);
			this.Offset = new Vector3(-Road.LaneWidth/2 + 0.05f, 0, 0.05f);
			this.Rotation = Matrix.CreateFromYawPitchRoll(0, 0.02f, 0);
            this.lane = lane;
		}

		public void Update(GameTime gameTime)
		{
		}

        public new void Render(GameTime gameTime, Effect effect)
        {
            if (HighlightOn == lane)
            {
                effect.Parameters["ChromaKeyReplace"].SetValue(Color.Gold.ToVector4());
            } else
            {
                effect.Parameters["ChromaKeyReplace"].SetValue(Color.DarkOliveGreen.ToVector4());
            }
            base.Render(gameTime, effect);
        }
	}
}
