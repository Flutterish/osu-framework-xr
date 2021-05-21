using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using static osu.Framework.XR.Maths.Extensions;
using osuTK;
using System;
using OpenVR.NET;

namespace osu.Framework.XR.Physics {
	// TODO all of the physics methods should include both a regular and a prenormalized version for preformance
	public static class Raycast {
		/// <summary>
		/// Intersect a 3D line and a place.
		/// </summary>
		public static bool TryHit ( Vector3 origin, Vector3 direction, Vector3 pointOnPlane, Vector3 planeNormal, out RaycastHit hit, bool includeBehind = false ) { // BUG something is wrong with some collisions ( flat Y normals )
			// plane := all points where ( point - pointOnPlane ) dot planeNormal = 0
			// line := all points where ( point - pointOnLine ) - d * direction = 0
			// in other words, point = pointOnLine + d * direction
			// the intersection of the two: ( pointOnLine + d * direction - pointOnPlane ) dot planeNormal = 0
			// direction dot planeNormal * d + ( pointOnLine - pointOnPlane ) dot planeNormal = 0
			// d = (( pointOnPlane - pointOnLine ) dot planeNormal) / (direction dot planeNormal)
			// therefore if direction dot planeNormal is 0, there is no intersection or they are on top of each other
			direction.Normalize();
			planeNormal.Normalize();

			var dot = Vector3.Dot( direction, planeNormal );
			if ( dot == 0 ) {
				if ( Vector3.Dot( origin - pointOnPlane, planeNormal ) == 0 ) {
					hit = new RaycastHit(
						origin,
						origin,
						planeNormal,
						direction,
						0
					);
					return true;
				}
				else {
					hit = default;
					return false;
				}
			}
			else {
				var distance = Vector3.Dot( pointOnPlane - origin, planeNormal ) / dot;

				hit = new RaycastHit(
					origin + direction * distance,
					origin,
					planeNormal,
					direction,
					distance
				);
				return distance >= 0 || includeBehind;
			}
		}

		public static (Vector3 pointOnA, Vector3 pointOnB) FindClosestPointsBetween2Lines ( Vector3 pointOnLineA, Vector3 directionA, Vector3 pointOnLineB, Vector3 directionB ) {
			// https://www.gamedev.net/forums/topic/520233-closest-point-on-a-line-to-another-line-in-3d/
			var d = pointOnLineB - pointOnLineA;
			var v1d = Vector3.Dot( directionA, d );
			var v2d = Vector3.Dot( directionB, d );
			var v1v2 = Vector3.Dot( directionA, directionB );
			var v1v1 = Vector3.Dot( directionA, directionA );
			var v2v2 = Vector3.Dot( directionB, directionB );

			var b = ( v2d - v1v2 * v1d / v1v1 ) / ( v1v2 * v1v2 / v1v1 - v2v2 );
			var a = ( v1d + v1v2 * b ) / v1v1;

			return ( pointOnLineA + a * directionA, pointOnLineB + b * directionB );
		}

		/// <summary>
		/// Intersect 2 3D lines.
		/// </summary>
		public static bool TryHitLine ( Vector3 pointOnLineA, Vector3 directionA, Vector3 pointOnLineB, Vector3 directionB, out Vector3 hit ) {
			Vector3 b;
			(hit, b) = FindClosestPointsBetween2Lines( pointOnLineA, directionA.Normalized(), pointOnLineB, directionB.Normalized() );
			return (hit - b).LengthSquared < 0.01f;
		}

		/// <summary>
		/// Intersect 2 2D lines.
		/// </summary>
		public static bool TryHitLine ( Vector2 pointOnLineA, Vector2 directionA, Vector2 pointOnLineB, Vector2 directionB, out Vector2 hit ) {
			// y = m1 x + b1
			// y = m2 x + b2
			// m1 x + b1 = m2 x + b2
			// (b1-b2) = x(m2-m1)
			// x = (m2-m1)/(b1-b2)
			// y - m1 x = b1

			if ( directionA.X == 0 ) {
				var m1 = directionA.X / directionA.Y;
				var b1 = pointOnLineA.X - m1 * pointOnLineA.Y;
				var m2 = directionB.X / directionB.Y;
				var b2 = pointOnLineB.X - m2 * pointOnLineB.Y;

				if ( m1 == m2 ) {
					if ( b1 == b2 ) {
						hit = pointOnLineA;
						return true;
					}
					else {
						hit = default;
						return false;
					}
				}
				else {
					var y = ( b1 - b2 ) / ( m2 - m1 );
					hit = new Vector2( m1 * y + b1, y );
					return true;
				}
			}
			else {
				var m1 = directionA.Y / directionA.X;
				var b1 = pointOnLineA.Y - m1 * pointOnLineA.X;
				var m2 = directionB.Y / directionB.X;
				var b2 = pointOnLineB.Y - m2 * pointOnLineB.X;

				if ( m1 == m2 ) {
					if ( b1 == b2 ) {
						hit = pointOnLineA;
						return true;
					}
					else {
						hit = default;
						return false;
					}
				}
				else {
					var x = ( b1 - b2 ) / ( m2 - m1 );
					hit = new Vector2( x, m1 * x + b1 );
					return true;
				}
			}
		}

		/// <summary>
		/// Intersect a 3D line and a triangle.
		/// </summary>
		public static bool TryHit ( Vector3 origin, Vector3 direction, Face face, out RaycastHit hit, bool includeBehind = false ) {
			var normal = Vector3.Cross( face.B - face.A, face.C - face.A );
			// we want the normal to be pointing towards the hit origin
			if ( Vector3.Dot( normal, direction ) > 0 ) {
				normal *= -1;
			}

			if ( TryHit( origin, direction, face.A, normal, out hit, includeBehind ) ) {
				var directionFromC = ( face.C - hit.Point ).Normalized();
				if ( TryHitLine( hit.Point, directionFromC, face.A, face.B - face.A, out var pointOnAB ) ) {
					var distanceFromAToB = SignedDistance( face.A, pointOnAB, face.B );
					if ( distanceFromAToB >= -0.01f && distanceFromAToB <= ( face.B - face.A ).Length + 0.01f ) {
						var distanceToC = SignedDistance( face.C, hit.Point, pointOnAB );
						if ( distanceToC >= -0.01f && distanceToC <= ( face.C - pointOnAB ).Length + 0.01f ) {
							return true;
						}
					}
				}
			}

			hit = default;
			return false;
		}

		public static bool Intersects ( Vector3 origin, Vector3 direction, AABox box, bool includeBehind = false ) {
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
		/// Intersect a 3D line and a Mesh.
		/// </summary>
		public static bool TryHit ( Vector3 origin, Vector3 direction, Mesh mesh, Transform transform, out RaycastHit hit, bool includeBehind = false ) {
			if ( mesh.Tris.Count > 6 ) {
				if ( !Intersects( origin, direction, transform.Matrix * mesh.BoundingBox, includeBehind ) ) {
					hit = default;
					return false;
				}
			}
			RaycastHit? closest = null;

			for ( int i = 0; i < mesh.Tris.Count; i++ ) {
				var face = mesh.Faces[ i ];
				face.A = ( transform.Matrix * new Vector4( face.A, 1 ) ).Xyz;
				face.B = ( transform.Matrix * new Vector4( face.B, 1 ) ).Xyz;
				face.C = ( transform.Matrix * new Vector4( face.C, 1 ) ).Xyz;
				if ( TryHit( origin, direction, face, out hit, includeBehind ) && ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) ) {
					closest = new RaycastHit(
						hit.Point,
						hit.Origin,
						hit.Normal,
						hit.Direction,
						hit.Distance,
						i
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

		/// <summary>
		/// Intersect a 3D line and a Mesh.
		/// </summary>
		public static bool TryHit ( Vector3 origin, Vector3 direction, Mesh mesh, Transform transform, ReadonlyIndexer<int, Face> indexer, out RaycastHit hit, bool includeBehind = false ) {
			if ( mesh.Tris.Count > 6 ) {
				if ( !Intersects( origin, direction, transform.Matrix * mesh.BoundingBox, includeBehind ) ) {
					hit = default;
					return false;
				}
			}
			RaycastHit? closest = null;

			for ( int i = 0; i < mesh.Tris.Count; i++ ) {
				var face = indexer[ i ];
				if ( TryHit( origin, direction, face, out hit, includeBehind ) && ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) ) {
					closest = new RaycastHit(
						hit.Point,
						hit.Origin,
						hit.Normal,
						hit.Direction,
						hit.Distance,
						i
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

		/// <summary>
		/// Intersect a 3D line and a Mesh.
		/// </summary>
		public static bool TryHit ( Vector3 origin, Vector3 direction, Model target, out RaycastHit hit, bool includeBehind = false ) {
			var ok = TryHit( origin, direction, target.Mesh, target.Transform, target.Faces, out hit, includeBehind );
			if ( ok ) {
				hit = new RaycastHit(
					hit.Point,
					hit.Origin,
					hit.Normal,
					hit.Direction,
					hit.Distance,
					hit.TrisIndex,
					target as IHasCollider
				);
			}
			return ok;
		}

		/// <summary>
		/// The closest point to a line [from;to]
		/// </summary>
		public static Vector3 ClosestPoint ( Vector3 from, Vector3 to, Vector3 other ) {
			var dir = to - from;
			TryHit( from, dir, other, dir, out var hit, true );

			// P = from + (to-from) * T -> T = (P - from)/(to-from);
			float t;
			if ( dir.X != 0 ) t = ( hit.Point.X - from.X ) / dir.X;
			else if ( dir.Y != 0 ) t = ( hit.Point.Y - from.Y ) / dir.Y;
			else t = ( hit.Point.Z - from.Z ) / dir.Z;

			if ( t < 0 ) return from;
			else if ( t > 1 ) return to;
			else return hit.Point;
		}

		public readonly struct RaycastHit {
			/// <summary>
			/// The point that was hit.
			/// </summary>
			public readonly Vector3 Point;
			/// <summary>
			/// From where the raycast originated.
			/// </summary>
			public readonly Vector3 Origin;
			/// <summary>
			/// The normal of the hit surface.
			/// </summary>
			public readonly Vector3 Normal;
			/// <summary>
			/// The direction of the raycast.
			/// </summary>
			public readonly Vector3 Direction;
			/// <summary>
			/// Distance from the origin to the hit point. Might be negative if the hit happened in opposite direction.
			/// </summary>
			public readonly double Distance;
			/// <summary>
			/// The triangle of the mesh that was hit, if any.
			/// </summary>
			public readonly int TrisIndex;
			/// <summary>
			/// The hit collider, if any.
			/// </summary>
			public readonly IHasCollider? Collider;

			public RaycastHit ( Vector3 point, Vector3 origin, Vector3 normal, Vector3 direction, double distance, int trisIndex = -1, IHasCollider? collider = null ) {
				Point = point;
				Origin = origin;
				Normal = normal;
				Direction = direction;
				Distance = distance;
				TrisIndex = trisIndex;
				Collider = collider;
			}
		}
	}
}
