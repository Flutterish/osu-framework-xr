namespace osu.Framework.XR.Allocation;

public interface IUpload : IDisposable {
	/// <summary>
	/// Uploads the data on the draw thread. This resource will be disposed immediately after the call to this method
	/// </summary>
	void Upload ();
}
