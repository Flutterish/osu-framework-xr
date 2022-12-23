using osu.Framework.XR.Graphics.Meshes;

namespace osu.Framework.XR.Parsing;

/// <summary>
/// Imported mesh, not uploaded automatically. It might share buffers with other imported meshes - to upload it, you should use <see cref="ImportedMeshCollection.CreateUploadForAllMeshes"/>
/// </summary>
public class ImportedMesh {
	public string? Name;
	public readonly Mesh Mesh;

	public ImportedMesh ( Mesh mesh ) {
		Mesh = mesh;
	}
}
