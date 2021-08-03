using osu.Framework.Utils;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Valve.VR;

namespace osu.Framework.XR.Maths {
	public static class Extensions {
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

		public static Vector3 With ( this Vector3 v, float? x = null, float? y = null, float? z = null )
			=> new Vector3( x ?? v.X, y ?? v.Y, z ?? v.Z );

		public static float SignedDistance ( Vector3 from, Vector3 to, Vector3 towards ) {
			var direction = to - from;
			return direction.Length * ( Vector3.Dot( direction, towards - from ) > 0 ? 1 : -1 );
		}

		public static Vector2 ScaledBy ( this Vector2 a, Vector2 scale )
			=> new Vector2( a.X * scale.X, a.Y * scale.Y );

		public static Vector3 ExtractPosition ( this HmdMatrix34_t mat ) {
			return new Vector3( mat.m3, mat.m7, -mat.m11 );
		}

		public static Vector3 ExtractPosition ( this Matrix4x4 mat ) {
			return new Vector3( mat.M30, mat.M31, mat.M32 );
		}

		public static Quaternion ExtractRotation ( this HmdMatrix34_t mat ) {
			static float CopySign ( float a, float b ) {
				if ( MathF.Sign( a ) != MathF.Sign( b ) )
					return -a;
				else return a;
			}

			Quaternion q = default;
			q.W = MathF.Sqrt( MathF.Max( 0, 1 + mat.m0 + mat.m5 + mat.m10 ) ) / 2;
			q.X = MathF.Sqrt( MathF.Max( 0, 1 + mat.m0 - mat.m5 - mat.m10 ) ) / 2;
			q.Y = MathF.Sqrt( MathF.Max( 0, 1 - mat.m0 + mat.m5 - mat.m10 ) ) / 2;
			q.Z = MathF.Sqrt( MathF.Max( 0, 1 - mat.m0 - mat.m5 + mat.m10 ) ) / 2;
			q.X = CopySign( q.X, mat.m9 - mat.m6 );
			q.Y = CopySign( q.Y, mat.m2 - mat.m8 );
			q.Z = CopySign( q.Z, mat.m1 - mat.m4 );
			return q.Normalized().Inverted();
		}

		public static double NextDouble ( this Random random, double range )
			=> random.NextDouble() * range;

		public static double NextDouble ( this Random random, double from, double to )
			=> from + random.NextDouble() * ( to - from );

		public static bool Chance ( this Random random, double chance )
			=> random.NextDouble() < chance;

		public static Vector3 Average<T> ( this IEnumerable<T> a, Func<T, Vector3> selector )
			=> new Vector3(
				a.Average( x => selector( x ).X ),
				a.Average( x => selector( x ).Y ),
				a.Average( x => selector( x ).Z )
			);

		/// <summary>
		/// Decomposes a quaternion into into its rotation around an axis.
		/// To calculate the remaining rotation use `totalRotation * result.Inverted()`.
		/// </summary>
		public static Quaternion DecomposeAroundAxis ( this Quaternion quaternion, Vector3 axis ) {
			var rotationAxis = new Vector3( quaternion.X, quaternion.Y, quaternion.Z );
			var dot = Vector3.Dot( axis, rotationAxis );
			var projected = axis * dot;

			var twist = new Quaternion( projected.X, projected.Y, projected.Z, quaternion.W ).Normalized();

			if ( twist.LengthSquared == 0 ) {
				twist = Quaternion.Identity;
			}
			else if ( dot < 0 ) {
				return twist * -1;
			}

			return twist;
		}

		/// <summary>
		/// A quaternion such that Z+ would align with the given direction
		/// </summary>
		public static Quaternion LookRotation ( this Vector3 direction ) {
			direction.Normalize();

			return direction.Y == 1
				? Quaternion.FromEulerAngles( MathF.PI / 2, 0, 0 )
				: direction.Y == -1
				? Quaternion.FromEulerAngles( -MathF.PI / 2, 0, 0 )
				: Matrix4.LookAt( Vector3.Zero, direction, Vector3.UnitY ).ExtractRotation().Inverted();
		}

		public static Quaternion ShortestRotationTo ( this Vector3 from, Vector3 to ) { // TODO with an up vector
			var dot = from.Dot( to );
			if ( dot < -0.999999 ) {
				return Quaternion.FromAxisAngle( from.AnyOrthogonal(), MathF.PI );
			}
			else if ( dot > 0.999999 ) {
				return Quaternion.Identity;
			}
			else {
				return new Quaternion( from.Cross( to ), 1 + dot ).Normalized();
			}
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

		/// <summary>
		/// A quaternion such that Z+ and Y+ would align with the given direction
		/// </summary>
		public static Quaternion LookRotation ( this Vector3 direction, Vector3 up ) {
			direction.Normalize();

			return Matrix4.LookAt( Vector3.Zero, direction, Vector3.UnitY ).ExtractRotation().Inverted();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Apply ( this Quaternion quaternion, Vector3 vector )
			=> ( quaternion * new Vector4( vector, 1 ) ).Xyz;
	}
}
