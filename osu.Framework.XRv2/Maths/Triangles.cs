namespace osu.Framework.XR.Maths;

public static class Triangles {
	/// <summary>
	/// Calculates the barycentric coordinates of a point that lies on a face.
	/// If the point does not lie on the face, the results will be less accurate.
	/// </summary>
	public static Vector3 BarycentricFast ( Face face, Vector3 point ) {
		var normal = Vector3.Cross( face.A - face.B, face.C - face.B );
		var dotX = MathF.Abs( Vector3.Dot( normal, Vector3.UnitX ) );
		var dotY = MathF.Abs( Vector3.Dot( normal, Vector3.UnitY ) );
		var dotZ = MathF.Abs( Vector3.Dot( normal, Vector3.UnitZ ) );

		// choosing the least distorting plane
		if ( dotZ > dotX && dotZ > dotY ) {
			return Barycentric( face.A.Xy, face.B.Xy, face.C.Xy, point.Xy );
		}
		else if ( dotY > dotX ) {
			return Barycentric( face.A.Xz, face.B.Xz, face.C.Xz, point.Xz );
		}
		else {
			return Barycentric( face.A.Yz, face.B.Yz, face.C.Yz, point.Yz );
		}
	}

	/// <summary>
	/// Calculates the barycentric coordinates of a point that lies on a face.
	/// If the point does not lie on the face, the behaviour is undefined.
	/// </summary>
	public static Vector3 BarycentricFast ( Vector3 A, Vector3 B, Vector3 C, Vector3 point )
		=> BarycentricFast( new Face( A, B, C ), point );

	/// <summary>
	/// Calculates the barycentric coordinates of a point with respect to a face.
	/// If the point does not lie on the face, it will be cast onto it. If you know the point does lie on the face, use <see cref="BarycentricFast(Face, Vector3)"/>.
	/// </summary>
	public static Vector3 Barycentric ( Face face, Vector3 point ) {
		var normal = Vector3.Cross( face.A - face.B, face.C - face.B );
		var d4 = Barycentric( face.A, face.B, face.C, face.Centre + normal, point );

		return d4.Xyz / ( 1 - d4.W );
	}

	/// <summary>
	/// Calculates the barycentric coordinates of a point with respect to a face.
	/// If the point does not lie on the face, it will be cast onto it. If you know the point does lie on the face, use <see cref="BarycentricFast(Face, Vector3)"/>.
	/// </summary>
	public static Vector3 Barycentric ( Vector3 A, Vector3 B, Vector3 C, Vector3 point )
		=> Barycentric( new Face( A, B, C ), point );

	/// <summary>
	/// Calculates the barycentric coordinates of a point on a simplex.
	/// </summary>
	public static Vector3 Barycentric ( Vector2 A, Vector2 B, Vector2 C, Vector2 point ) {
		var bycy = B.Y - C.Y;
		var axcx = A.X - C.X;
		var cxbx = C.X - B.X;
		var aycy = A.Y - C.Y;
		var pycy = point.Y - C.Y;
		var pxcx = point.X - C.X;

		var det = bycy * axcx + cxbx * aycy;
		var r1 = ( bycy * pxcx + cxbx * pycy ) / det;
		var r2 = ( axcx * pycy - aycy * pxcx ) / det;
		return new Vector3( r1, r2, 1 - r1 - r2 );
	}

	/// <summary>
	/// Calculates the barycentric coordinates of a point on a simplex.
	/// </summary>
	public static Vector4 Barycentric ( Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 point ) {
		// A * a + B * b + C * c + D * ( 1 - a - b - c ) = p

		// a * ( A - D ) + b * ( B - D ) + c * ( C - D ) = p - D

		// [ (A-D).x (B-D).x (C-D).x ]   [ a ]   [ (p-D).x ]
		// [ (A-D).y (B-D).y (C-D).y ] * [ b ] = [ (p-D).y ]
		// [ (A-D).z (B-D).z (C-D).z ]   [ c ]   [ (p-D).z ]
		// T * w = p - D
		// w = T^-1 * (p - D)

		var matrix = new Matrix3( A - D, B - D, C - D );
		matrix.Transpose();
		matrix.Invert();

		var weights = matrix * ( point - D );
		return new Vector4( weights, 1 - weights.X - weights.Y - weights.Z );
	}

	/// <summary>
	/// Checks whether a given point is inside a triangle, given that the point lies on the triangle plane already.
	/// If it does not, the bahaviour is undefined.
	/// </summary>
	public static bool IsPointInside ( Vector3 p, Face face ) {
		var b = BarycentricFast( face, p );
		return b.X >= 0 && b.Y >= 0 && b.Z >= 0;
	}

	public static bool IsPointInside ( Vector3 p, Vector3 A, Vector3 B, Vector3 C, Vector3 D ) {
		var b = Barycentric( A, B, C, D, p );
		return b.X >= 0 && b.Y >= 0 && b.Z >= 0 && b.W >= 0;
	}
}