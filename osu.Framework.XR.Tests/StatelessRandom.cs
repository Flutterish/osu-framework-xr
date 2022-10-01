using osuTK;
using System;

namespace osu.Framework.XR.Tests;

public static class StatelessRandom {
	public static Vector3 RandomVector ( int seed, float xmin = -1, float xmax = 1, float ymin = -1, float ymax = 1, float zmin = -1, float zmax = 1 ) {
		var random = new Random( seed );
		return new Vector3(
			random.NextSingle( xmin, xmax ),
			random.NextSingle( ymin, ymax ),
			random.NextSingle( zmin, zmax )
		);
	}
}