using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Allocation;

namespace osu.Framework.XR;

public static class UploadScheduler {
	static object mutex = new();
	static Queue<IUpload> queue = new();

	// NOTE consider a separate queue for the update thread (no lock), which is then moved
	// to the draw thread queue (lock) at the end of the update frame
	public static void Enqueue ( IUpload upload ) {
		lock ( mutex ) {
			queue.Enqueue( upload );
		}
	}

	public static void Execute ( IRenderer renderer, int limit = int.MaxValue ) {
		lock ( mutex ) {
			while ( limit-- > 0 && queue.TryDequeue( out var upload ) ) {
				upload.Upload( renderer );
			}
		}
	}
}
