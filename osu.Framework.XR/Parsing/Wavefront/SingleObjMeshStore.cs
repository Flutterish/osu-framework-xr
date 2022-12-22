using osu.Framework.IO.Stores;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.IO.Stores;

namespace osu.Framework.XR.Parsing.Wavefront;

public class SingleObjMeshStore : StringParsingStore<Mesh> {
	public SingleObjMeshStore ( IResourceStore<byte[]> store ) : base( store ) {
		Store.AddExtension( "obj" );
	}

	protected override Mesh Parse ( string data ) {
		return SimpleObjFile.Load( data );
	}
}
