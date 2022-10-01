using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

public interface ITriangleMesh : IGeometryMesh {
	int TriangleCount { get; }
	(uint indexA, uint indexB, uint indexC) GetTriangleIndices ( int index );
}

public static class ITriangleMeshExtensions {
	public static Face GetTriangleFace ( this ITriangleMesh self, int triangleIndex ) {
		var (a, b, c) = self.GetTriangleIndices( triangleIndex );
		return new(
			self.GetVertexPosition( a ),
			self.GetVertexPosition( b ),
			self.GetVertexPosition( c )
		);
	}
}