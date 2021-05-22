using osu.Framework.XR.Physics;
using osuTK;

namespace osu.Framework.XR.Maths {
	public static class Vectors {
		/// <summary>
		/// Computes the closest point on the direction line to this point.
		/// <paramref name="direction"/> must be a normal vector.
		/// </summary>
		public static Vector3 AlignedWithPrenormalized ( this Vector3 vector, Vector3 direction ) {
			Raycast.TryHitPrenormalized( Vector3.Zero, direction, vector, direction, out var hit, true );
			return hit.Point;
		}
		/// <summary>
		/// Computes the closest point on the direction line to this point.
		/// </summary>
		public static Vector3 AlignedWith ( this Vector3 vector, Vector3 direction )
			=> AlignedWithPrenormalized( vector, direction.Normalized() );

		/// <summary>
		/// Computes the shadow of this vector on a plane.
		/// <paramref name="normal"/> must be a normal vector.
		/// </summary>
		public static Vector3 ProjectedOnPrenormalized ( this Vector3 vector, Vector3 normal ) {
			return vector - vector.AlignedWithPrenormalized( normal );
		}
		/// <summary>
		/// Computes the shadow of this vector on a plane.
		/// </summary>
		public static Vector3 ProjectedOn ( this Vector3 vector, Vector3 normal )
			=> ProjectedOnPrenormalized( vector, normal.Normalized() );
	}
}
