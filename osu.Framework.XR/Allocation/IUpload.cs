using osu.Framework.Graphics.Rendering;

namespace osu.Framework.XR.Allocation;

public interface IUpload {
	/// <summary>
	/// Uploads the data on the draw thread
	/// </summary>
	void Upload ( IRenderer renderer );

	/// <summary>
	/// Enqueues this <see cref="IUpload"/> to the default <see cref="UploadScheduler"/>
	/// </summary>
	public void Enqueue ()
		=> UploadScheduler.Enqueue( this );
}

public static class IUploadExtensions {
	/// <inheritdoc cref="IUpload.Enqueue"/>
	public static void Enqueue ( this IUpload upload ) {
		upload.Enqueue();
	}
}

public class CombinedUpload : IUpload {
	RentedArray<IUpload> uploads;

	/// <summary>
	/// Combines multiple uploads into a single operation. 
	/// The provided rented array will be disposed after the upload is complete
	/// </summary>
	public CombinedUpload ( RentedArray<IUpload> uploads ) {
		this.uploads = uploads;
	}

	void IUpload.Upload ( IRenderer renderer ) {
		foreach ( var i in uploads )
			i.Upload( renderer );

		uploads.Dispose();
	}
}

public class DelegateUpload : IUpload {
	Action action;

	public DelegateUpload ( Action action ) {
		this.action = action;
	}

	public void Upload ( IRenderer renderer ) {
		action();
	}
}

public class DelegateRendererUpload : IUpload {
	Action<IRenderer> action;

	public DelegateRendererUpload ( Action<IRenderer> action ) {
		this.action = action;
	}

	public void Upload ( IRenderer renderer ) {
		action( renderer );
	}
}

public class DelegateUpload<T> : IUpload {
	T context;
	Action<T> action;

	public DelegateUpload ( T context, Action<T> action ) {
		this.context = context;
		this.action = action;
	}

	public void Upload ( IRenderer renderer ) {
		action( context );
	}
}

public class DelegateRendererUpload<T> : IUpload {
	T context;
	Action<T, IRenderer> action;

	public DelegateRendererUpload ( T context, Action<T, IRenderer> action ) {
		this.context = context;
		this.action = action;
	}

	public void Upload ( IRenderer renderer ) {
		action( context, renderer );
	}
}