using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Physics;

public static class Sphere {
	public static bool TryHit ( Vector3 origin, double radius, Face face, out SphereHit hit ) {
		Vector3 normal = Vector3.Cross( face.A - face.B, face.C - face.B ).Normalized();
		if ( !Raycast.TryHitPrenormalized( origin, normal, face.A, normal, out var rh, true ) ) {
			hit = default;
			return false;
		}

		if ( Triangles.IsPointInside( rh.Point, face ) ) {
			if ( Math.Abs( rh.Distance ) <= radius ) {
				hit = new SphereHit {
					Origin = origin,
					Normal = rh.Normal,
					Radius = radius,
					Point = rh.Point
				};
				return true;
			}
			else {
				hit = default;
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
				hit = default;
				return false;
			}
			else if ( al < bl && al < cl ) {
				hit = new SphereHit {
					Origin = origin,
					Normal = rh.Normal,
					Radius = radius,
					Point = A
				};
				return true;
			}
			else if ( bl < cl ) {
				hit = new SphereHit {
					Origin = origin,
					Normal = rh.Normal,
					Radius = radius,
					Point = B
				};
				return true;
			}
			else {
				hit = new SphereHit {
					Origin = origin,
					Normal = rh.Normal,
					Radius = radius,
					Point = C
				};
				return true;
			}
		}
	}

	public static bool TryHit ( Vector3 origin, double radius, ITriangleMesh mesh, Matrix4 transform, out SphereHit hit ) {
		var aabb = mesh.BoundingBox * transform;
		if ( ( aabb.Min + aabb.Size / 2 - origin ).Length > aabb.Size.Length + radius ) {
			hit = default;
			return false;
		}

		SphereHit? closest = null;
		var tris = mesh.TriangleCount;
		for ( int i = 0; i < tris; i++ ) {
			var face = mesh.GetTriangleFace( i );
			face.A = transform.Apply( face.A );
			face.B = transform.Apply( face.B );
			face.C = transform.Apply( face.C );
			if ( TryHit( origin, radius, face, out hit ) && ( closest is null || closest.Value.Distance > hit.Distance ) ) {
				closest = hit with { TrisIndex = i };
			}
		}

		if ( closest is null ) {
			hit = default;
			return false;
		}
		else {
			hit = closest.Value;
			return true;
		}
	}

	public static bool TryHit ( Vector3 origin, double radius, ITriangleMesh mesh, out SphereHit hit ) {
		var aabb = mesh.BoundingBox;
		if ( ( aabb.Min + aabb.Size / 2 - origin ).Length > aabb.Size.Length + radius ) {
			hit = default;
			return false;
		}

		SphereHit? closest = null;
		var tris = mesh.TriangleCount;
		for ( int i = 0; i < tris; i++ ) {
			var face = mesh.GetTriangleFace( i );
			if ( TryHit( origin, radius, face, out hit ) && ( closest is null || closest.Value.Distance > hit.Distance ) ) {
				closest = hit with { TrisIndex = i };
			}
		}

		if ( closest is null ) {
			hit = default;
			return false;
		}
		else {
			hit = closest.Value;
			return true;
		}
	}

	public static bool TryHit ( Vector3 origin, double radius, IHasCollider target, out SphereHit hit ) {
		if ( TryHit( origin, radius, target.ColliderMesh, out hit ) ) {
			hit = hit with { Collider = target };
			return true;
		}
		return false;
	}
}

public readonly struct SphereHit {
	/// <summary>
	/// The point that was hit.
	/// </summary>
	public Vector3 Point { get; init; }
	/// <summary>
	/// The origin of the sphere.
	/// </summary>
	public Vector3 Origin { get; init; }
	/// <summary>
	/// The normal of the surface which was hit.
	/// </summary>
	public Vector3 Normal { get; init; }
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
	public double Radius { get; init; }
	/// <summary>
	/// The triangle of the mesh that was hit, if any.
	/// </summary>
	public int TrisIndex { get; init; }
	/// <summary>
	/// The hit collider, if any.
	/// </summary>
	public IHasCollider? Collider { get; init; }
}