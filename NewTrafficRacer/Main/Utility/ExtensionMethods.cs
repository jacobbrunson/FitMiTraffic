using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Utility
{
	public static class ExtensionMethods
	{
		public static float Map(this float value, float fromSource, float toSource, float fromTarget, float toTarget, bool clamp=false)
		{
			if (clamp)
			{
				if (value < fromSource)
				{
					value = fromSource;
				} else if (value > toSource)
				{
					value = toSource;
				}
			}
			return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
		}

		public static float Map(this int v, float fromSource, float toSource, float fromTarget, float toTarget, bool clamp = false)
		{
			float value = v;
			if (clamp)
			{
				if (value < fromSource)
				{
					value = fromSource;
				}
				else if (value > toSource)
				{
					value = toSource;
				}
			}
			return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
		}

		public static float Map(this double value, float fromSource, float toSource, float fromTarget, float toTarget, bool clamp = false)
		{
			if (clamp)
			{
				if (value < fromSource)
				{
					value = fromSource;
				}
				else if (value > toSource)
				{
					value = toSource;
				}
			}
			return ((float)value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
		}
	}
}
