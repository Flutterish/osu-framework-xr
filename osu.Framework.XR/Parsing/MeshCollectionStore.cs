using osu.Framework.IO.Stores;
using System.Collections.Concurrent;

namespace osu.Framework.XR.Parsing;

/// <summary>
/// Provides <see cref="ImportedScene"/>s sourced from any number of provided sources.
/// </summary>
public class MeshCollectionStore : ResourceStore<ImportedScene> {
	/// <summary>
	/// Returns a shared instance of a scene. This instance will be automatically uploaded.
	/// </summary>
	public override ImportedScene Get ( string name ) {
		return GetAsync( name ).Result;
	}

	ConcurrentDictionary<string, Task<ImportedScene>> loadTasks = new();

	/// <summary>
	/// Returns a shared instance of a scene asynchronously. This instance will be automatically uploaded.
	/// </summary>
	public override Task<ImportedScene> GetAsync ( string name, CancellationToken cancellationToken = default ) {
		return loadTasks.GetOrAdd( name, async name => {
			var newMesh = await base.GetAsync( name );
			newMesh.CreateUploadForAllMeshes().Enqueue();
			return newMesh;
		} );
	}

	/// <summary>
	/// Returns a new instance of a scene.
	/// </summary>
	public ImportedScene GetNew ( string name ) {
		return base.Get( name );
	}

	/// <summary>
	/// Returns a new instance of a scene asynchronously.
	/// </summary>
	public Task<ImportedScene> GetNewAsync ( string name, CancellationToken cancellationToken = default ) {
		return base.GetAsync( name );
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var i in loadTasks.Values ) {
			i.Result?.DisposeBuffers();
		}

		base.Dispose( disposing );
	}
}
