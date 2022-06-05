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
				hit = new SphereHit(
					distance: Math.Abs( rh.Distance ),
					origin: origin,
					radius: radius,
					point: rh.Point
				);
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
				hit = new SphereHit(
					distance: al,
					origin: origin,
					radius: radius,
					point: A
				);
				return true;
			}
			else if ( bl < cl ) {
				hit = new SphereHit(
					distance: bl,
					origin: origin,
					radius: radius,
					point: B
				);
				return true;
			}
			else {
				hit = new SphereHit(
					distance: cl,
					origin: origin,
					radius: radius,
					point: C
				);
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
				closest = new SphereHit(
					distance: hit.Distance,
					origin: hit.Origin,
					radius: hit.Radius,
					point: hit.Point,
					trisIndex: i
				);
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
		if ( TryHit( origin, radius, target.Mesh, target.Matrix, out hit ) ) {
			hit = new SphereHit(
				distance: hit.Distance,
				origin: hit.Origin,
				radius: hit.Radius,
				point: hit.Point,
				trisIndex: hit.TrisIndex,
				collider: target
			);
			return true;
		}
		return false;
	}
}

public readonly struct SphereHit {
	public readonly double Distance;
	public readonly Vector3 Origin;
	public readonly double Radius;
	public readonly Vector3 Point;
	public readonly int TrisIndex;
	public readonly IHasCollider? Collider;

	public SphereHit ( double distance, Vector3 origin, double radius, Vector3 point, int trisIndex = -1, IHasCollider? collider = null ) {
		Distance = distance;
		Origin = origin;
		Radius = radius;
		Point = point;
		TrisIndex = trisIndex;
		Collider = collider;
	}
}