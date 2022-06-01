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