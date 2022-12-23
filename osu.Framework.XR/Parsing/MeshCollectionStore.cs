using osu.Framework.IO.Stores;
using System.Collections.Concurrent;

namespace osu.Framework.XR.Parsing;

/// <summary>
/// Provides <see cref="ImportedMeshCollection"/>s sourced from any number of provided sources.
/// </summary>
public class MeshCollectionStore : ResourceStore<ImportedMeshCollection> {
	/// <summary>
	/// Returns a shared instance of a mesh collection. This instance will be automatically uploaded.
	/// </summary>
	public override ImportedMeshCollection Get ( string name ) {
		return GetAsync( name ).Result;
	}

	ConcurrentDictionary<string, Task<ImportedMeshCollection>> loadTasks = new();

	/// <summary>
	/// Returns a shared instance of a mesh collection asynchronously. This instance will be automatically uploaded.
	/// </summary>
	public override Task<ImportedMeshCollection> GetAsync ( string name, CancellationToken cancellationToken = default ) {
		return loadTasks.GetOrAdd( name, async name => {
			var newMesh = await base.GetAsync( name );
			newMesh.CreateUploadForAllMeshes().Enqueue();
			return newMesh;
		} );
	}

	/// <summary>
	/// Returns a new instance of a mesh collection.
	/// </summary>
	public ImportedMeshCollection GetNew ( string name ) {
		return base.Get( name );
	}

	/// <summary>
	/// Returns a new instance of a mesh collection asynchronously.
	/// </summary>
	public Task<ImportedMeshCollection> GetNewAsync ( string name, CancellationToken cancellationToken = default ) {
		return base.GetAsync( name, cancellationToken );
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var i in loadTasks.Values ) {
			i.Result?.DisposeBuffers();
		}

		base.Dispose( disposing );
	}
}
