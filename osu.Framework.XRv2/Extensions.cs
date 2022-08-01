using System.Runtime.InteropServices;

namespace osu.Framework.XR;

public static class Extensions {
	/// <inheritdoc cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>
	public static Span<T> AsSpan<T> ( this List<T> list )
		=> CollectionsMarshal.AsSpan( list );

	public static Vector3 Apply ( this Quaternion quaternion, Vector3 vector )
		=> ( quaternion * new Vector4( vector, 1 ) ).Xyz;

	public static Vector3 Apply ( this Matrix4 matrix, Vector3 vector )
		=> ( new Vector4( vector, 1 ) * matrix ).Xyz;

	/// <summary>
	/// A quaternion such that a unit Z vector would align with the given direction 
	/// after applying the quaternion
	/// </summary>
	public static Quaternion LookRotation ( this Vector3 direction ) {
		direction.Normalize();

		return direction.Y == 1
			? Quaternion.FromEulerAngles( -MathF.PI / 2, 0, 0 )
			: direction.Y == -1
			? Quaternion.FromEulerAngles( MathF.PI / 2, 0, 0 )
			: Matrix4.LookAt( Vector3.Zero, -direction, Vector3.UnitY ).ExtractRotation().Inverted();
	}

	public static Vector3 ToEuler ( this Quaternion q ) {
		// using https://gamedev.net/forums/topic/597324-quaternion-to-euler-angles-and-back-why-is-the-rotation-changing/4784042/
		var xSquare = q.X * q.X;
		var ySquare = q.Y * q.Y;
		var zSquare = q.Z * q.Z;
		var wSquare = q.W * q.W;
		return new Vector3(
			MathF.Atan2( -2 * ( q.Y * q.Z - q.W * q.X ), wSquare - xSquare - ySquare + zSquare ),
			MathF.Asin( 2 * ( q.X * q.Z + q.W * q.Y ) ),
			MathF.Atan2( -2 * ( q.X * q.Y - q.W * q.Z ), wSquare + xSquare - ySquare - zSquare )
		);
	}

	public static float Dot ( this Vector3 a, Vector3 b )
		=> Vector3.Dot( a, b );

	public static Vector3 Cross ( this Vector3 a, Vector3 b )
		=> Vector3.Cross( a, b );

	/// <summary>
	/// Returns some normal orthogonal vector
	/// </summary>
	public static Vector3 AnyOrthogonal ( this Vector3 vector ) {
		var cross = vector.Cross( Vector3.UnitX );
		if ( ( cross - vector ).LengthSquared < 0.0000001 )
			cross = vector.Cross( Vector3.UnitZ );

		return cross.Normalized();
	}

	public static double NextDouble ( this Random random, double from, double to )
			=> from + random.NextDouble() * ( to - from );

	public static float NextSingle ( this Random random, float from, float to )
		=> from + random.NextSingle() * ( to - from );

	public static float NextSingle ( this Random random )
		=> (float)random.NextDouble();

	public static IEnumerable<string> SplitLines ( this string text ) {
		using StringReader reader = new( text );
		while ( reader.ReadLine() is string str ) {
			yield return str;
		}
	}

	public static T At<T> ( this IList<T> self, int index, T @default = default! )
		=> self.Count > index ? self[index] : @default;

	public static Vector3 ToOsuTk ( this System.Numerics.Vector3 vec )
		=> new( vec.X, vec.Y, vec.Z );
	public static Quaternion ToOsuTk ( this System.Numerics.Quaternion quat )
		=> new( quat.X, quat.Y, quat.Z, quat.W );
	public static Matrix4 ToOsuTk ( this System.Numerics.Matrix4x4 mat ) {
		var m = new Matrix4() {
			M11 = mat.M11,
			M12 = mat.M12,
			M13 = mat.M13,
			M14 = mat.M14,
			M21 = mat.M21,
			M22 = mat.M22,
			M23 = mat.M23,
			M24 = mat.M24,
			M31 = mat.M31,
			M32 = mat.M32,
			M33 = mat.M33,
			M34 = mat.M34,
			M41 = mat.M41,
			M42 = mat.M42,
			M43 = mat.M43,
			M44 = mat.M44
		};
		m.Transpose();
		return m;
	}

	public static float Mod ( this float x, float y ) {
		x = x % y;
		if ( x < 0 )
			return y + x;
		return x;
	}

	public static float AngleDistance ( this float from, float to ) {
		return ( to - from + MathF.PI ).Mod( MathF.Tau ) - MathF.PI;
	}
}
