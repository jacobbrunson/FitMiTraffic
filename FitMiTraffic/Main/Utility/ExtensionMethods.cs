using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Utility
{
	public static class ExtensionMethods
	{
		public static float Map(this float value, float fromSource, float toSource, float fromTarget, float toTarget)
		{
			return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
		}

		public static float Map(this int value, float fromSource, float toSource, float fromTarget, float toTarget)
		{
			return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
		}

		public static float Map(this double value, float fromSource, float toSource, float fromTarget, float toTarget)
		{
			return ((float)value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
		}
	}
}
