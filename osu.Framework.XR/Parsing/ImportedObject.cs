namespace osu.Framework.XR.Parsing;

/// <summary>
/// An imported object possibly consisting of meshes and materials
/// </summary>
public class ImportedObject {
	public string? Name;
	public Vector3 Position;
	public Quaternion Rotation = Quaternion.Identity;
	public Vector3 Scale = Vector3.One;
	public ImportedObject? TransformParent;

	public readonly List<ImportedMeshPart> MeshParts = new();
}

/// <summary>
/// Object mesh part consisting of a mesh and optionally a material
/// </summary>
public struct ImportedMeshPart {
	public string? Name;
	public ImportedMesh Mesh;
	public ImportedMaterial? Material;
}