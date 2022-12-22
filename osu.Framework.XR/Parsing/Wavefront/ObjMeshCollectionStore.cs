using osu.Framework.IO.Stores;
using osu.Framework.XR.IO.Stores;

namespace osu.Framework.XR.Parsing.Wavefront;

public class ObjMeshCollectionStore : StringParsingStore<ImportedScene> {
	public ObjMeshCollectionStore ( IResourceStore<byte[]> store ) : base( store ) {
		Store.AddExtension( "obj" );
	}

	protected override ImportedScene Parse ( string data ) {
		return ObjFile.Load( data );
	}
}
