using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

public interface ITriangleMesh : IMesh {
	int TriangleCount { get; }
	(uint indexA, uint indexB, uint indexC) GetTriangleIndices ( int index );
	Vector3 GetTriangleVertex ( uint index );

	public Face GetTriangleFace ( int triaangleIndex ) {
		var (a, b, c) = GetTriangleIndices( triaangleIndex );
		return new(
			GetTriangleVertex( a ),
			GetTriangleVertex( b ),
			GetTriangleVertex( c )
		);
	}
}