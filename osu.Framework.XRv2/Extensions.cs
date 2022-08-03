namespace osu.Framework.XR;

public static class Extensions {
	public static double NextDouble ( this Random random, double from, double to )
		=> from + random.NextDouble() * ( to - from );

	public static float NextSingle ( this Random random, float from, float to )
		=> from + random.NextSingle() * ( to - from );

	public static float NextSingle ( this Random random )
		=> (float)random.NextDouble();

	public static IEnumerable<string> SplitLines ( this string text ) {
		using StringReader reader = new( text );
		while ( reader.ReadLine() is string str ) {
			yield return str;
		}
	}
}
