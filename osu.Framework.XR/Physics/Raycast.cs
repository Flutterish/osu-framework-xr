﻿using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Physics;

// it works. its fine.
#pragma warning disable CS9093 // This ref-assigns a value that can only escape the current method through a return statement.
public static class Raycast {
	[ThreadStatic]
	static RaycastHit swapHit; // used for temp values with ref swapping

	/// <summary>
	/// Intersect a ray and a plane.
	/// <paramref name="direction"/> and <paramref name="planeNormal"/> must be normal vectors.
	/// </summary>
	public static bool TryHitPrenormalized ( Vector3 origin, Vector3 direction, Vector3 pointOnPlane, Vector3 planeNormal, ref RaycastHit hit, bool includeBehind = false ) {
		// plane := all points where ( point - pointOnPlane ) dot planeNormal = 0
		// line := all points where ( point - pointOnLine ) - d * direction = 0
		// in other words, point = pointOnLine + d * direction
		// the intersection of the two: ( pointOnLine + d * direction - pointOnPlane ) dot planeNormal = 0
		// direction dot planeNormal * d + ( pointOnLine - pointOnPlane ) dot planeNormal = 0
		// d = (( pointOnPlane - pointOnLine ) dot planeNormal) / (direction dot planeNormal)
		// therefore if direction dot planeNormal is 0, there is no intersection or they are on top of each other

		var dot = Vector3.Dot( direction, planeNormal );
		if ( dot == 0 ) {
			if ( Vector3.Dot( origin - pointOnPlane, planeNormal ) == 0 ) {
				hit.Point = origin;
				hit.Origin = origin;
				hit.Normal = planeNormal;
				hit.Direction = direction;
				return true;
			}
			else {
				return false;
			}
		}
		else {
			var distance = Vector3.Dot( pointOnPlane - origin, planeNormal ) / dot;
			hit.Point = origin + direction * distance;
			hit.Origin = origin;
			hit.Normal = planeNormal;
			hit.Direction = direction;
			hit.Distance = distance;
			return distance >= 0 || includeBehind;
		}
	}
	/// <summary>
	/// Intersect a ray and a plane.
	/// </summary>
	public static bool TryHit ( Vector3 origin, Vector3 direction, Vector3 pointOnPlane, Vector3 planeNormal, ref RaycastHit hit, bool includeBehind = false )
		=> TryHitPrenormalized( origin, direction.Normalized(), pointOnPlane, planeNormal.Normalized(), ref hit, includeBehind );

	/// <summary>
	/// Finds points on both lines that are closest to the other line.
	/// <paramref name="directionA"/> and <paramref name="directionB"/> must be normal vectors.
	/// </summary>
	public static (Vector3 pointOnA, Vector3 pointOnB) FindClosestPointsBetween2RaysPrenormalized ( Vector3 pointOnLineA, Vector3 directionA, Vector3 pointOnLineB, Vector3 directionB ) {
		// https://www.gamedev.net/forums/topic/520233-closest-point-on-a-line-to-another-line-in-3d/
		var d = pointOnLineB - pointOnLineA;
		var v1d = Vector3.Dot( directionA, d );
		var v2d = Vector3.Dot( directionB, d );
		var v1v2 = Vector3.Dot( directionA, directionB );

		var b = ( v2d - v1v2 * v1d ) / ( v1v2 * v1v2 - 1 );
		var a = v1d + v1v2 * b;

		return (pointOnLineA + a * directionA, pointOnLineB + b * directionB);
	}
	/// <summary>
	/// Finds points on both lines that are closest to the other line.
	/// </summary>
	public static (Vector3 pointOnA, Vector3 pointOnB) FindClosestPointsBetween2Rays ( Vector3 pointOnLineA, Vector3 directionA, Vector3 pointOnLineB, Vector3 directionB )
		=> FindClosestPointsBetween2RaysPrenormalized( pointOnLineA, directionA.Normalized(), pointOnLineB, directionB.Normalized() );

	/// <summary>
	/// Intersect 2 3D lines.
	/// <paramref name="directionA"/> and <paramref name="directionB"/> must be normal vectors.
	/// </summary>
	/// <param name="tolerance">How close the rays need to be to intersect. For performance reasons, this is the square of the actual tolerance.</param>
	public static bool TryHitRayPrenormalized ( Vector3 pointOnLineA, Vector3 directionA, Vector3 pointOnLineB, Vector3 directionB, out Vector3 hit, float tolerance = 0.000001f ) {
		Vector3 b;
		(hit, b) = FindClosestPointsBetween2RaysPrenormalized( pointOnLineA, directionA, pointOnLineB, directionB );
		return ( hit - b ).LengthSquared <= tolerance;
	}
	/// <summary>
	/// Intersect 2 3D lines.
	/// </summary>
	public static bool TryHitRay ( Vector3 pointOnLineA, Vector3 directionA, Vector3 pointOnLineB, Vector3 directionB, out Vector3 hit, float tolerance = 0.001f )
		=> TryHitRayPrenormalized( pointOnLineA, directionA.Normalized(), pointOnLineB, directionB.Normalized(), out hit, tolerance * tolerance );

	/// <summary>
	/// Intersect a ray and a triangle.
	/// <paramref name="direction"/> must be a normal vector.
	/// </summary>
	public static bool TryHitPrenormalized ( Vector3 origin, Vector3 direction, Face face, ref RaycastHit hit, bool includeBehind = false ) {
		var normal = Vector3.Cross( face.B - face.A, face.C - face.A );
		// we want the normal to be pointing towards the hit origin
		if ( Vector3.Dot( normal, direction ) > 0 ) {
			normal *= -1;
		}

		if ( TryHitPrenormalized( origin, direction, face.A, normal.Normalized(), ref hit, includeBehind ) ) {
			return Triangles.IsPointInside( hit.Point, face );
		}

		return false;
	}
	/// <summary>
	/// Intersect a ray and a triangle.
	/// </summary>
	public static bool TryHit ( Vector3 origin, Vector3 direction, Face face, ref RaycastHit hit, bool includeBehind = false )
		=> TryHitPrenormalized( origin, direction.Normalized(), face, ref hit, includeBehind );

	/// <summary>
	/// Checks if a ray intersects an axis aligned box.
	/// </summary>
	public static bool Intersects ( Vector3 origin, Vector3 direction, AABox box, bool includeBehind = false ) {
		// TODO simplify this? also use includeBehind
		if ( direction.X == 0 ) {
			if ( origin.X < box.Min.X || origin.X > box.Max.X ) return false;
		}
		else {
			var tA = ( box.Min.X - origin.X ) / direction.X;
			var tB = ( box.Max.X - origin.X ) / direction.X;

			var aPoint = origin + direction * tA;
			var bPoint = origin + direction * tB;

			if ( aPoint.Y < box.Min.Y && bPoint.Y < box.Min.Y || aPoint.Y > box.Max.Y && bPoint.Y > box.Max.Y ) return false;
			if ( aPoint.Z < box.Min.Z && bPoint.Z < box.Min.Z || aPoint.Z > box.Max.Z && bPoint.Z > box.Max.Z ) return false;
		}
		if ( direction.Y == 0 ) {
			if ( origin.Y < box.Min.Y || origin.Y > box.Max.Y ) return false;
		}
		else {
			var tA = ( box.Min.Y - origin.Y ) / direction.Y;
			var tB = ( box.Max.Y - origin.Y ) / direction.Y;

			var aPoint = origin + direction * tA;
			var bPoint = origin + direction * tB;

			if ( aPoint.X < box.Min.X && bPoint.X < box.Min.X || aPoint.X > box.Max.X && bPoint.X > box.Max.X ) return false;
			if ( aPoint.Z < box.Min.Z && bPoint.Z < box.Min.Z || aPoint.Z > box.Max.Z && bPoint.Z > box.Max.Z ) return false;
		}
		if ( direction.Z == 0 ) {
			if ( origin.Z < box.Min.Z || origin.Z > box.Max.Z ) return false;
		}
		else {
			var tA = ( box.Min.Z - origin.Z ) / direction.Z;
			var tB = ( box.Max.Z - origin.Z ) / direction.Z;

			var aPoint = origin + direction * tA;
			var bPoint = origin + direction * tB;

			if ( aPoint.Y < box.Min.Y && bPoint.Y < box.Min.Y || aPoint.Y > box.Max.Y && bPoint.Y > box.Max.Y ) return false;
			if ( aPoint.X < box.Min.X && bPoint.X < box.Min.X || aPoint.X > box.Max.X && bPoint.X > box.Max.X ) return false;
		}

		return true;
	}

	/// <summary>
	/// Intersect a ray and a Mesh.
	/// <paramref name="direction"/> must be a normal vector.
	/// </summary>
	public static bool TryHitPrenormalized ( Vector3 origin, Vector3 direction, ITriangleMesh mesh, Matrix4 transform, ref RaycastHit hit, bool includeBehind = false ) {
		var tris = mesh.TriangleCount;
		if ( tris > 6 ) {
			if ( !Intersects( origin, direction, mesh.BoundingBox * transform, includeBehind ) ) {
				return false;
			}
		}

		bool hasResult = false;
		ref RaycastHit closest = ref swapHit;
		ref RaycastHit swap = ref hit;

		for ( int i = 0; i < tris; i++ ) {
			var face = mesh.GetTriangleFace( i );
			face.A = transform.Apply( face.A );
			face.B = transform.Apply( face.B );
			face.C = transform.Apply( face.C );
			if ( TryHitPrenormalized( origin, direction, face, ref swap, includeBehind ) && ( !hasResult || Math.Abs( closest.Distance ) > Math.Abs( swap.Distance ) ) ) {
				ref RaycastHit temp = ref closest;
				unsafe { closest = ref swap; }
				swap = ref temp;
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
	/// <summary>
	/// Intersect a ray and a Mesh.
	/// </summary>
	public static bool TryHit ( Vector3 origin, Vector3 direction, ITriangleMesh mesh, Matrix4 transform, ref RaycastHit hit, bool includeBehind = false )
		=> TryHitPrenormalized( origin, direction.Normalized(), mesh, transform, ref hit, includeBehind );

	/// <summary>
	/// Intersect a ray and a Mesh.
	/// <paramref name="direction"/> must be a normal vector.
	/// </summary>
	public static bool TryHitPrenormalized ( Vector3 origin, Vector3 direction, ITriangleMesh mesh, ref RaycastHit hit, bool includeBehind = false ) {
		var tris = mesh.TriangleCount;
		if ( tris > 6 ) {
			if ( !Intersects( origin, direction, mesh.BoundingBox, includeBehind ) ) {
				return false;
			}
		}

		bool hasResult = false;
		ref RaycastHit closest = ref swapHit;
		ref RaycastHit swap = ref hit;

		for ( int i = 0; i < tris; i++ ) {
			var face = mesh.GetTriangleFace( i );
			if ( TryHitPrenormalized( origin, direction, face, ref swap, includeBehind ) && ( !hasResult || Math.Abs( closest.Distance ) > Math.Abs( swap.Distance ) ) ) {
				ref RaycastHit temp = ref closest;
				unsafe { closest = ref swap; }
				swap = ref temp;
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
	/// <summary>
	/// Intersect a ray and a Mesh.
	/// </summary>
	public static bool TryHit ( Vector3 origin, Vector3 direction, ITriangleMesh mesh, ref RaycastHit hit, bool includeBehind = false )
		=> TryHitPrenormalized( origin, direction.Normalized(), mesh, ref hit, includeBehind );

	/// <summary>
	/// Intersect a ray and a Mesh.
	/// <paramref name="direction"/> must be a normal vector.
	/// </summary>
	public static bool TryHitPrenormalized ( Vector3 origin, Vector3 direction, IHasCollider target, ref RaycastHit hit, bool includeBehind = false ) {
		if ( TryHitPrenormalized( origin, direction, target.ColliderMesh, ref hit, includeBehind ) ) {
			hit.Collider = target;
			return true;
		}
		return false;
	}
	/// <summary>
	/// Intersect a ray and a Mesh.
	/// </summary>
	public static bool TryHit ( Vector3 origin, Vector3 direction, IHasCollider target, ref RaycastHit hit, bool includeBehind = false )
		=> TryHitPrenormalized( origin, direction.Normalized(), target, ref hit, includeBehind );

	/// <summary>
	/// The closest point to a line [from;to]
	/// </summary>
	public static Vector3 ClosestPoint ( Vector3 from, Vector3 to, Vector3 other ) {
		var dir = to - from;
		var ndir = dir.Normalized();

		RaycastHit hit = new();
		TryHitPrenormalized( from, ndir, other, ndir, ref hit, true );
		Vector3 point = hit.Point;

		// P = from + (to-from) * T -> T = (P - from)/(to-from);
		float t;
		if ( dir.X != 0 ) t = ( point.X - from.X ) / dir.X;
		else if ( dir.Y != 0 ) t = ( point.Y - from.Y ) / dir.Y;
		else t = ( point.Z - from.Z ) / dir.Z;

		if ( t < 0 ) return from;
		else if ( t > 1 ) return to;
		else return point;
	}
}

public struct RaycastHit {
	/// <summary>
	/// The point that was hit.
	/// </summary>
	public Vector3 Point;
	/// <summary>
	/// From where the raycast originated.
	/// </summary>
	public Vector3 Origin;
	/// <summary>
	/// The normal of the surface which was hit.
	/// </summary>
	public Vector3 Normal;
	/// <summary>
	/// The direction of the raycast.
	/// </summary>
	public Vector3 Direction;
	/// <summary>
	/// Distance from the origin to the hit point. Can be negative to indicate the ray travelled backward with regard to <see cref="Direction"/>.
	/// </summary>
	public float Distance;
	/// <summary>
	/// The triangle of the mesh that was hit, if any.
	/// </summary>
	public int TrisIndex;
	/// <summary>
	/// The hit collider, if any.
	/// </summary>
	public IHasCollider? Collider;
}