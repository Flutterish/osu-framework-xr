using osu.Framework.IO.Stores;
using osu.Framework.XR.IO.Stores;

namespace osu.Framework.XR.Parsing.Wavefront;

public class ObjMeshCollectionStore : StringParsingStore<ImportedMeshCollection> {
	public ObjMeshCollectionStore ( IResourceStore<byte[]> store ) : base( store ) {
		Store.AddExtension( "obj" );
	}

	protected override ImportedMeshCollection Parse ( string data ) {
		return ObjFile.Load( data );
	}
}
