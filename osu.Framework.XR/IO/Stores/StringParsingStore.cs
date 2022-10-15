using osu.Framework.IO.Stores;
using System.Text;

namespace osu.Framework.XR.IO.Stores;

public abstract class StringParsingStore<T> : IResourceStore<T> where T : class {
	protected readonly ResourceStore<byte[]> Store;

	public StringParsingStore ( IResourceStore<byte[]> store ) {
		Store = new ResourceStore<byte[]>( store );
	}

	protected virtual string? GetData ( string name ) {
		var data = Store.Get( name );
		if ( data is null )
			return null;

		return Encoding.UTF8.GetString( data );
	}
	protected virtual async Task<string?> GetDataAsync ( string name ) {
		var data = await Store.GetAsync( name );
		if ( data is null )
			return null;

		return Encoding.UTF8.GetString( data );
	}

	protected abstract T Parse ( string data );

	public virtual T Get ( string name ) {
		var data = GetData( name );
		if ( data is null )
			return null!;

		return Parse( data );
	}
	public virtual async Task<T> GetAsync ( string name, CancellationToken cancellationToken = default ) {
		var data = await GetDataAsync( name );
		if ( data is null )
			return null!;

		return Parse( data );
	}

	public virtual Stream GetStream ( string name ) {
		return Store.GetStream( name );
	}

	public virtual IEnumerable<string> GetAvailableResources () {
		return Store.GetAvailableResources();
	}

	public void Dispose () {
		Dispose( true );
		GC.SuppressFinalize( this );
	}

	private bool isDisposed;
	protected virtual void Dispose ( bool disposing ) {
		if ( isDisposed )
			return;

		Store.Dispose();
		isDisposed = true;
	}
}
