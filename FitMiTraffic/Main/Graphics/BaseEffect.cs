using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Graphics
{
	public class BaseEffect
	{
		public Effect Effect;

		private Matrix view;
		private Matrix projection;
		private RenderTarget2D shadowMap;

		public Matrix LightViewProjection;
		public Matrix View
		{
			get { return view; }
			set { view = value; Effect.Parameters["View"].SetValue(value); }
		}

		public Matrix Projection
		{
			get { return projection; }
			set { projection = value; Effect.Parameters["Projection"].SetValue(value); }
		}

		public RenderTarget2D ShadowMap
		{
			get { return shadowMap; }
			set { shadowMap = value; Effect.Parameters["ShadowMap"].SetValue(value); }
		}

		public EffectParameterCollection Parameters
		{
			get { return Effect.Parameters; }
		}

		public EffectTechnique Technique
		{
			get { return Effect.CurrentTechnique; }
			set { Effect.CurrentTechnique = value; }
		}

		public EffectTechniqueCollection Techniques
		{
			get { return Effect.Techniques; }
		}

		public BaseEffect(Effect effect)
		{
			Effect = effect;
		}
	}
}
