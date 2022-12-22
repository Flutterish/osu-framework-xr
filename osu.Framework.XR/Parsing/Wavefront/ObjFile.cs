namespace osu.Framework.XR.Parsing.Wavefront;

public static class ObjFile {
	public static ImportedScene Load ( string data ) => Load( data.SplitLines() );
	public static ImportedScene Load ( IEnumerable<string> lines ) {
		ImportedScene scene = new();


		return scene;
	}
}
