namespace osu.Framework.XR.Allocation;

public interface IUpload {
	/// <summary>
	/// Uploads the data on the draw thread
	/// </summary>
	void Upload ();

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

	void IUpload.Upload () {
		foreach ( var i in uploads )
			i.Upload();

		uploads.Dispose();
	}
}

public class DelegateUpload : IUpload {
	Action action;

	public DelegateUpload ( Action action ) {
		this.action = action;
	}

	public void Upload () {
		action();
	}
}

public class DelegateUpload<T> : IUpload {
	T context;
	Action<T> action;

	public DelegateUpload ( T context, Action<T> action ) {
		this.context = context;
		this.action = action;
	}

	public void Upload () {
		action( context );
	}
}