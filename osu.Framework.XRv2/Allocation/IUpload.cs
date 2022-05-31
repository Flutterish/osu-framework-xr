namespace osu.Framework.XR.Allocation;

public interface IUpload {
	/// <summary>
	/// Uploads the data on the draw thread
	/// </summary>
	void Upload ();
}
