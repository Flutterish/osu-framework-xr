using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

public interface IGeometryMesh : IHasBoundingBox {
	uint VertexCount { get; }
	Vector3 GetVertexPosition ( uint index );
}

public interface IHasBoundingBox {
	AABox BoundingBox { get; }
}