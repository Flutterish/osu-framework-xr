using System;

namespace osu.Framework.XR.Extensions {
	public static class RandomExtensions {
		public static double NextDouble ( this Random random, double from, double to )
			=> from + random.NextDouble() * ( to - from );

		public static float NextSingle ( this Random random, float from, float to )
			=> from + random.NextSingle() * ( to - from );

		public static float NextSingle ( this Random random )
			=> (float)random.NextDouble();
	}
}
