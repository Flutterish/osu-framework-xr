using osu.Framework.IO.Stores;
using osu.Framework.XR.IO.Stores;
using osu.Framework.XR.Parsing.Wavefront;

namespace osu.Framework.XR.Graphics.Meshes;

public class ObjMeshLoaderStore : StringParsingStore<Mesh> {
	public ObjMeshLoaderStore ( IResourceStore<byte[]> store ) : base( store ) {
		Store.AddExtension( "obj" );
	}

	protected override Mesh Parse ( string data ) {
		return SimpleObjFile.Load( data );
	}
}
