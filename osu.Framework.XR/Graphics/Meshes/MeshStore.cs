using osu.Framework.IO.Stores;
using osu.Framework.XR.Parsing;

namespace osu.Framework.XR.Graphics.Meshes;

/// <summary>
/// Provides drawable-ready <see cref="Mesh"/>es and <see cref="ImportedScene"/>s sourced from any number of provided sources.
/// </summary>
public class MeshStore : IResourceStore<Mesh>, IResourceStore<ImportedScene>, IDisposable {
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
	public ImportedScene GetScene ( string name )
		=> MeshCollectionStore.Get(name );

	/// <inheritdoc cref="MeshCollectionStore.GetAsync(string, CancellationToken)"/>
	public Task<ImportedScene> GetSceneAsync ( string name, CancellationToken cancellationToken = default )
		=> MeshCollectionStore.GetAsync( name, cancellationToken );

	/// <inheritdoc cref="MeshCollectionStore.GetNew(string)"/>
	public ImportedScene GetNewScene ( string name )
		=> MeshCollectionStore.GetNew( name );

	/// <inheritdoc cref="MeshCollectionStore.GetNewAsync(string, CancellationToken)"/>
	public Task<ImportedScene> GetNewSceneAsync ( string name, CancellationToken cancellationToken = default )
		=> MeshCollectionStore.GetNewAsync( name, cancellationToken );

	ImportedScene IResourceStore<ImportedScene>.Get ( string name ) 
		=> GetScene( name );

	Task<ImportedScene> IResourceStore<ImportedScene>.GetAsync ( string name, CancellationToken cancellationToken )
		=> GetSceneAsync( name, cancellationToken );

	Stream IResourceStore<ImportedScene>.GetStream ( string name )
		=> MeshCollectionStore.GetStream( name );

	IEnumerable<string> IResourceStore<ImportedScene>.GetAvailableResources ()
		=> MeshCollectionStore.GetAvailableResources();

	public void AddStore ( IResourceStore<ImportedScene> store )
		=> MeshCollectionStore.AddStore( store );
	#endregion

	public void Dispose () {
		SingleMeshStore.Dispose();
		MeshCollectionStore.Dispose();
	}
}