using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Physics;

// it works. its fine.
#pragma warning disable CS9093 // This ref-assigns a value that can only escape the current method through a return statement.
public static class Sphere {
	[ThreadStatic]
	static SphereHit swapHit; // used for temp values with ref swapping

	public static bool TryHit ( Vector3 origin, double radius, Face face, ref SphereHit hit ) {
		Vector3 normal = Vector3.Cross( face.A - face.B, face.C - face.B ).Normalized();
		RaycastHit rh = new();
		if ( !Raycast.TryHitPrenormalized( origin, normal, face.A, normal, ref rh, true ) ) {
			return false;
		}

		if ( Triangles.IsPointInside( rh.Point, face ) ) {
			if ( Math.Abs( rh.Distance ) <= radius ) {
				hit.Origin = origin;
				hit.Normal = rh.Normal;
				hit.Radius = radius;
				hit.Point = rh.Point;
				return true;
			}
			else {
				return false;
			}
		}
		else {
			var A = Raycast.ClosestPoint( face.A, face.B, rh.Point );
			var B = Raycast.ClosestPoint( face.B, face.C, rh.Point );
			var C = Raycast.ClosestPoint( face.C, face.A, rh.Point );

			var al = ( A - origin ).Length;
			var bl = ( B - origin ).Length;
			var cl = ( C - origin ).Length;

			if ( al > radius && bl > radius && cl > radius ) {
				return false;
			}
			else if ( al < bl && al < cl ) {
				hit.Origin = origin;
				hit.Normal = rh.Normal;
				hit.Radius = radius;
				hit.Point = A;
				return true;
			}
			else if ( bl < cl ) {
				hit.Origin = origin;
				hit.Normal = rh.Normal;
				hit.Radius = radius;
				hit.Point = B;
				return true;
			}
			else {
				hit.Origin = origin;
				hit.Normal = rh.Normal;
				hit.Radius = radius;
				hit.Point = C;
				return true;
			}
		}
	}

	public static bool TryHit ( Vector3 origin, double radius, ITriangleMesh mesh, Matrix4 transform, ref SphereHit hit ) {
		var aabb = mesh.BoundingBox * transform;
		if ( ( aabb.Min + aabb.Size / 2 - origin ).Length > aabb.Size.Length + radius ) {
			return false;
		}

		bool hasResult = false;
		ref SphereHit closest = ref hit;
		ref SphereHit swap = ref swapHit;

		var tris = mesh.TriangleCount;
		for ( int i = 0; i < tris; i++ ) {
			var face = mesh.GetTriangleFace( i );
			face.A = transform.Apply( face.A );
			face.B = transform.Apply( face.B );
			face.C = transform.Apply( face.C );
			if ( TryHit( origin, radius, face, ref swap ) && ( !hasResult || closest.Distance > swap.Distance ) ) {
				ref SphereHit temp = ref closest;
				closest = ref swap;
				unsafe { swap = ref temp; }
				closest.TrisIndex = i;
				hasResult = true;
			}
		}

		if ( hasResult ) {
			hit = closest;
			return true;
		}
		else {
			return false;
		}
	}

	public static bool TryHit ( Vector3 origin, double radius, ITriangleMesh mesh, ref SphereHit hit ) {
		var aabb = mesh.BoundingBox;
		if ( ( aabb.Min + aabb.Size / 2 - origin ).Length > aabb.Size.Length + radius ) {
			return false;
		}

		bool hasResult = false;
		ref SphereHit closest = ref hit;
		ref SphereHit swap = ref swapHit;

		var tris = mesh.TriangleCount;
		for ( int i = 0; i < tris; i++ ) {
			var face = mesh.GetTriangleFace( i );
			if ( TryHit( origin, radius, face, ref swap ) && ( !hasResult || closest.Distance > swap.Distance ) ) {
				ref SphereHit temp = ref closest;
				closest = ref swap;
				unsafe { swap = ref temp; }
				closest.TrisIndex = i;
				hasResult = true;
			}
		}

		if ( hasResult ) {
			hit = closest;
			return true;
		}
		else {
			return false;
		}
	}

	public static bool TryHit ( Vector3 origin, double radius, IHasCollider target, ref SphereHit hit ) {
		if ( TryHit( origin, radius, target.ColliderMesh, ref hit ) ) {
			hit.Collider = target;
			return true;
		}
		return false;
	}
}

public struct SphereHit {
	/// <summary>
	/// The point that was hit.
	/// </summary>
	public Vector3 Point;
	/// <summary>
	/// The origin of the sphere.
	/// </summary>
	public Vector3 Origin;
	/// <summary>
	/// The normal of the surface which was hit.
	/// </summary>
	public Vector3 Normal;
	/// <summary>
	/// The direction from the origin to the hit point.
	/// </summary>
	public Vector3 Direction => ( Point - Origin ).Normalized();
	/// <summary>
	/// Distance from the origin to the hit point.
	/// </summary>
	public double Distance => ( Point - Origin ).Length;
	/// <summary>
	/// The size of the sphere.
	/// </summary>
	public double Radius;
	/// <summary>
	/// The triangle of the mesh that was hit, if any.
	/// </summary>
	public int TrisIndex;
	/// <summary>
	/// The hit collider, if any.
	/// </summary>
	public IHasCollider? Collider;
}