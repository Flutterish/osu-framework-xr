using osu.Framework.XR.Graphics.Meshes;

namespace osu.Framework.XR.Maths;

/// <summary>
/// A 3D axis-aligned Box.
/// </summary>
public struct AABox {
	/// <summary>
	/// Origin located at X-, Y-, Z- most point of the box
	/// </summary>
	public Vector3 Min;
	/// <summary>
	/// How much the box expands into X+, Y+, Z+ from the origin.
	/// </summary>
	public Vector3 Size;
	public Vector3 Max => Min + Size;
	public Vector3 Center => Min + Size / 2;

	public AABox ( Span<Vector3> points ) {
		Vector3 min = new( float.PositiveInfinity );
		Vector3 max = new( float.NegativeInfinity );
		foreach ( var v in points ) {
			if ( v.X > max.X )
				max.X = v.X;
			if ( v.X < min.X )
				min.X = v.X;
			if ( v.Y > max.Y )
				max.Y = v.Y;
			if ( v.Y < min.Y )
				min.Y = v.Y;
			if ( v.Z > max.Z )
				max.Z = v.Z;
			if ( v.Z < min.Z )
				min.Z = v.Z;
		}

		Min = min;
		Size = max - min;
	}
	public AABox ( IEnumerable<Vector3> points ) {
		Vector3 min = new( float.PositiveInfinity );
		Vector3 max = new( float.NegativeInfinity );
		foreach ( var v in points ) {
			if ( v.X > max.X )
				max.X = v.X;
			if ( v.X < min.X )
				min.X = v.X;
			if ( v.Y > max.Y )
				max.Y = v.Y;
			if ( v.Y < min.Y )
				min.Y = v.Y;
			if ( v.Z > max.Z )
				max.Z = v.Z;
			if ( v.Z < min.Z )
				min.Z = v.Z;
		}

		Min = min;
		Size = max - min;
	}
	public AABox ( ITriangleMesh mesh ) {
		Vector3 min = new( float.PositiveInfinity );
		Vector3 max = new( float.NegativeInfinity );
		for ( uint i = 0; i < mesh.VertexCount; i++ ) {
			var v = mesh.GetVertexPosition( i );
			if ( v.X > max.X )
				max.X = v.X;
			if ( v.X < min.X )
				min.X = v.X;
			if ( v.Y > max.Y )
				max.Y = v.Y;
			if ( v.Y < min.Y )
				min.Y = v.Y;
			if ( v.Z > max.Z )
				max.Z = v.Z;
			if ( v.Z < min.Z )
				min.Z = v.Z;
		}

		Min = min;
		Size = max - min;
	}

	public static AABox operator * ( AABox box, Matrix4 matrix ) {
		var min = box.Min;
		var max = box.Max;

		Span<Vector3> points = stackalloc Vector3[] {
			matrix.Apply( new( min.X, min.Y, min.Z ) ),
			matrix.Apply( new( min.X, min.Y, max.Z ) ),
			matrix.Apply( new( min.X, max.Y, min.Z ) ),
			matrix.Apply( new( min.X, max.Y, max.Z ) ),
			matrix.Apply( new( max.X, min.Y, min.Z ) ),
			matrix.Apply( new( max.X, min.Y, max.Z ) ),
			matrix.Apply( new( max.X, max.Y, min.Z ) ),
			matrix.Apply( new( max.X, max.Y, max.Z ) )
		};

		min = new( float.PositiveInfinity );
		max = new( float.NegativeInfinity );
		foreach ( var v in points ) {
			if ( v.X > max.X )
				max.X = v.X;
			if ( v.X < min.X )
				min.X = v.X;
			if ( v.Y > max.Y )
				max.Y = v.Y;
			if ( v.Y < min.Y )
				min.Y = v.Y;
			if ( v.Z > max.Z )
				max.Z = v.Z;
			if ( v.Z < min.Z )
				min.Z = v.Z;
		}

		return new() { Min = min, Size = max - min };
	}
}