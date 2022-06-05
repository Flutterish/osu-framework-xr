namespace osu.Framework.XR.Graphics.Meshes;

public interface ITriangleMesh {
	IEnumerable<(uint indexA, uint indexB, uint indexC)> TriangleIndices { get; }
	Vector3 GetTriangleVertex ( uint index );
}
