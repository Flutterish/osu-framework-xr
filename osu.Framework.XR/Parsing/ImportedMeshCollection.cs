using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics.Buffers;

namespace osu.Framework.XR.Parsing;

/// <summary>
/// An imported mesh collection, including all meshes and their buffers (which are not uploaded automatically), objects and materials
/// </summary>
public class ImportedMeshCollection : ImportedGroup {
	public IUpload CreateUploadForAllMeshes () {
		var span = MemoryPool<IUpload>.Shared.Rent( AllElementBuffers.Count + AllVertexBuffers.Count );
		for ( int i = 0; i < AllElementBuffers.Count; i++ ) {
			span[i] = AllElementBuffers[i].CreateUpload();
		}
		for ( int i = 0; i < AllVertexBuffers.Count; i++ ) {
			span[i + AllElementBuffers.Count] = AllVertexBuffers[i].CreateUpload();
		}
		return new CombinedUpload( span );
	}
	public IUpload CreateUnsafeUploadForAllMeshes () {
		var span = MemoryPool<IUpload>.Shared.Rent( AllElementBuffers.Count + AllVertexBuffers.Count );
		for ( int i = 0; i < AllElementBuffers.Count; i++ ) {
			span[i] = AllElementBuffers[i].CreateUnsafeUpload();
		}
		for ( int i = 0; i < AllVertexBuffers.Count; i++ ) {
			span[i + AllElementBuffers.Count] = AllVertexBuffers[i].CreateUnsafeUpload();
		}
		return new CombinedUpload( span );
	}

	public readonly List<ImportedMesh> AllMeshes = new();
	public readonly List<ImportedMaterial> AllMaterials = new();
	public readonly List<ImportedObject> AllObjects = new();
	public readonly List<ImportedGroup> AllCollections = new();

	public readonly List<IElementBuffer> AllElementBuffers = new();
	public readonly List<IVertexBuffer> AllVertexBuffers = new();

	public void DisposeBuffers () {
		foreach ( var i in AllElementBuffers )
			i.Dispose();
		foreach ( var i in AllVertexBuffers )
			i.Dispose();
	}
}
