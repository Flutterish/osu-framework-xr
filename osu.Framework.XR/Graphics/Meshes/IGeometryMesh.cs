using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

public interface IGeometryMesh {
	uint VertexCount { get; }
	Vector3 GetVertexPosition ( uint index );
	AABox BoundingBox { get; }
}
