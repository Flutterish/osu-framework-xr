﻿using osu.Framework.XR.Graphics;
using osuTK;
using System;

namespace osu.Framework.XR.Maths {
	public static class Triangles {
		/// <summary>
		/// Calculates the barycentric coordinates of a point that lies on a face.
		/// If the point does not lie on the face, it will return the barycentric coordinates with respect to the least distorted cardinal plane.
		/// </summary>
		public static Vector3 Barycentric ( Face face, Vector3 point ) {
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
		/// Checks whether a given point is inside a triangle, given that the point lies on the triangle plane already.
		/// </summary>
		public static bool IsPointInside ( Vector3 p, Face face ) {
			var b = Barycentric( face, p );
			return b.X >= 0 && b.Y >= 0 && b.Z >= 0;
		}
	}
}
