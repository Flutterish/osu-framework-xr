using osu.Framework.Development;

namespace osu.Framework.XR;

public static class DisposeScheduler {
	static object mutex = new();
	static Queue<Action> queue = new();

	// NOTE consider a separate queue for the update thread (no lock), which is then moved
	// to the draw thread queue (lock) at the end of the update frame
	public static void Enqueue<T> ( T src, Action<T> action ) {
		if ( ThreadSafety.IsDrawThread ) {
			action( src );
			return;
		}

		lock ( mutex ) {
			queue.Enqueue( () => action( src ) );
		}
	}

	public static void Execute ( int limit = int.MaxValue ) {
		lock ( mutex ) {
			while ( limit-- > 0 && queue.TryDequeue( out var action ) ) {
				action();
			}
		}
	}
}
