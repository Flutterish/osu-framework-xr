using osu.Framework.IO.Stores;
using osu.Framework.XR.Parsing;

namespace osu.Framework.XR.Graphics.Meshes;

/// <summary>
/// Provides drawable-ready <see cref="Mesh"/>es and <see cref="ImportedMeshCollection"/>s sourced from any number of provided sources.
/// </summary>
public class MeshStore : IResourceStore<Mesh>, IResourceStore<ImportedMeshCollection>, IDisposable {
	public readonly SingleMeshStore SingleMeshStore = new();
	public readonly MeshCollectionStore MeshCollectionStore = new();

	#region Single Mesh
	/// <inheritdoc cref="SingleMeshStore.Get{T}(string)"/>
	public T Get<T> ( string name ) where T : Mesh
		=> SingleMeshStore.Get<T>( name );

	/// <inheritdoc cref="SingleMeshStore.GetAsync{T}(string, CancellationToken)"/>
	public Task<T> GetAsync<T> ( string name, CancellationToken cancellationToken = default ) where T : Mesh
		=> SingleMeshStore.GetAsync<T>( name );

	/// <inheritdoc cref="SingleMeshStore.Get(string)"/>
	public Mesh Get ( string name )
		=> SingleMeshStore.Get( name );

	/// <inheritdoc cref="SingleMeshStore.GetAsync{T}(string, CancellationToken)"/>
	public Task<Mesh> GetAsync ( string name, CancellationToken cancellationToken = default )
		=> SingleMeshStore.GetAsync( name, cancellationToken );

	/// <inheritdoc cref="SingleMeshStore.GetNew{T}(string)"/>
	public T GetNew<T> ( string name ) where T : Mesh
		=> SingleMeshStore.GetNew<T>( name );

	/// <inheritdoc cref="SingleMeshStore.GetNewAsync{T}(string, CancellationToken)"/>
	public Task<T> GetNewAsync<T> ( string name, CancellationToken cancellationToken = default ) where T : Mesh
		=> SingleMeshStore.GetNewAsync<T>( name, cancellationToken );

	/// <inheritdoc cref="SingleMeshStore.GetNew(string)"/>
	public Mesh GetNew ( string name )
		=> SingleMeshStore.GetNew( name );

	/// <inheritdoc cref="SingleMeshStore.GetNewAsync(string, CancellationToken)"/>
	public Task<Mesh> GetNewAsync ( string name, CancellationToken cancellationToken = default )
		=> SingleMeshStore.GetNewAsync( name, cancellationToken );

	Stream IResourceStore<Mesh>.GetStream ( string name )
		=> SingleMeshStore.GetStream( name );

	IEnumerable<string> IResourceStore<Mesh>.GetAvailableResources ()
		=> SingleMeshStore.GetAvailableResources();

	public void AddStore ( IResourceStore<Mesh> store )
		=> SingleMeshStore.AddStore( store );
	#endregion
	#region Mesh Collection
	/// <inheritdoc cref="MeshCollectionStore.Get(string)"/>
	public ImportedMeshCollection GetCollection ( string name )
		=> MeshCollectionStore.Get(name );

	/// <inheritdoc cref="MeshCollectionStore.GetAsync(string, CancellationToken)"/>
	public Task<ImportedMeshCollection> GetCollectionAsync ( string name, CancellationToken cancellationToken = default )
		=> MeshCollectionStore.GetAsync( name, cancellationToken );

	/// <inheritdoc cref="MeshCollectionStore.GetNew(string)"/>
	public ImportedMeshCollection GetNewCollection ( string name )
		=> MeshCollectionStore.GetNew( name );

	/// <inheritdoc cref="MeshCollectionStore.GetNewAsync(string, CancellationToken)"/>
	public Task<ImportedMeshCollection> GetNewCollectionAsync ( string name, CancellationToken cancellationToken = default )
		=> MeshCollectionStore.GetNewAsync( name, cancellationToken );

	ImportedMeshCollection IResourceStore<ImportedMeshCollection>.Get ( string name ) 
		=> GetCollection( name );

	Task<ImportedMeshCollection> IResourceStore<ImportedMeshCollection>.GetAsync ( string name, CancellationToken cancellationToken )
		=> GetCollectionAsync( name, cancellationToken );

	Stream IResourceStore<ImportedMeshCollection>.GetStream ( string name )
		=> MeshCollectionStore.GetStream( name );

	IEnumerable<string> IResourceStore<ImportedMeshCollection>.GetAvailableResources ()
		=> MeshCollectionStore.GetAvailableResources();

	public void AddStore ( IResourceStore<ImportedMeshCollection> store )
		=> MeshCollectionStore.AddStore( store );
	#endregion

	public void Dispose () {
		SingleMeshStore.Dispose();
		MeshCollectionStore.Dispose();
	}
}