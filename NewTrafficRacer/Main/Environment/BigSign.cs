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
    public class BigSign : RenderedModel
    {
        private const string ModelName = "bigsign";
        private static readonly string[] tex_names = { "buckleup", "car", "comet", "doinggreat", "gameanddrive", "goodjob", "nice", "slowdown", "txbdc", "vroom", "watchout", "wow" };

        public static BigSign Instantiate(ContentManager content)
        {
            Random r = new Random();
            string tex_name = "orange_signs/" + tex_names[r.Next(tex_names.Length)];
            return new BigSign(content, tex_name);
        }

		private BigSign(ContentManager content, string tex_name) : base(content, ModelName, tex_name)
		{
			//this.Rotation = Matrix.Identity
			this.Offset = new Vector3(-6, 0, 0);
			this.Size = new Vector3(12, 1, 6);
		}
	}
}
