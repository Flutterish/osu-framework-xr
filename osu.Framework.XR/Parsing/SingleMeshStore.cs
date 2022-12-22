using osu.Framework.IO.Stores;
using osu.Framework.XR.Graphics.Meshes;
using System.Collections.Concurrent;

namespace osu.Framework.XR.Parsing;

/// <summary>
/// Provides drawable-ready <see cref="Mesh"/>es sourced from any number of provided sources.
/// </summary>
public class SingleMeshStore : ResourceStore<Mesh> {
	/// <summary>
	/// Returns a shared instance of a mesh. This instance will be automatically uploaded.
	/// </summary>
	public T Get<T> ( string name ) where T : Mesh {
		return (T)Get( name );
	}

	/// <summary>
	/// Returns a shared instance of a mesh asynchronously. This instance will be automatically uploaded.
	/// </summary>
	public async Task<T> GetAsync<T> ( string name, CancellationToken cancellationToken = default ) where T : Mesh {
		return (T)await GetAsync( name, cancellationToken );
	}

	/// <summary>
	/// Returns a shared instance of a mesh. This instance will be automatically uploaded.
	/// </summary>
	public override Mesh Get ( string name ) {
		return GetAsync( name ).Result;
	}

	ConcurrentDictionary<string, Task<Mesh>> loadTasks = new();

	/// <summary>
	/// Returns a shared instance of a mesh asynchronously. This instance will be automatically uploaded.
	/// </summary>
	public override Task<Mesh> GetAsync ( string name, CancellationToken cancellationToken = default ) {
		return loadTasks.GetOrAdd( name, async name => {
			var newMesh = await base.GetAsync( name );
			newMesh.CreateFullUpload().Enqueue();
			return newMesh;
		} );
	}

	/// <summary>
	/// Returns a new instance of a mesh.
	/// </summary>
	public T GetNew<T> ( string name ) where T : Mesh {
		return (T)GetNew( name );
	}

	/// <summary>
	/// Returns a new instance of a mesh asynchronously.
	/// </summary>
	public async Task<T> GetNewAsync<T> ( string name, CancellationToken cancellationToken = default ) where T : Mesh {
		return (T)await GetNewAsync( name, cancellationToken );
	}

	/// <summary>
	/// Returns a new instance of a mesh.
	/// </summary>
	public Mesh GetNew ( string name ) {
		return base.Get( name );
	}

	/// <summary>
	/// Returns a new instance of a mesh asynchronously.
	/// </summary>
	public Task<Mesh> GetNewAsync ( string name, CancellationToken cancellationToken = default ) {
		return base.GetAsync( name );
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var i in loadTasks.Values ) {
			i.Result?.Dispose();
		}

		base.Dispose( disposing );
	}
}
