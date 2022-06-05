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

	public static double NextDouble ( this Random random, double from, double to )
			=> from + random.NextDouble() * ( to - from );

	public static float NextSingle ( this Random random, float from, float to )
		=> from + random.NextSingle() * ( to - from );

	public static float NextSingle ( this Random random )
		=> (float)random.NextDouble();
}
