using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osuTK;
using System;

namespace osu.Framework.XR.Components {
	public abstract partial class Panel {
		/// <summary>
		/// Creates panel shapes facing the XY plane.
		/// </summary>
		public static class Shapes {
			public static void MakeFlat ( Mesh mesh, float width, float height ) {
				mesh.AddQuad(
					new Quad(
						new Vector3( -width / 2, height / 2, 0 ), new Vector3( width / 2, height / 2, 0 ),
						new Vector3( -width / 2, -height / 2, 0 ), new Vector3( width / 2, -height / 2, 0 )
					),
					new Vector2( 0, 1 ),
					new Vector2( 1, 1 ),
					new Vector2( 0, 0 ),
					new Vector2( 1, 0 )
				);
			}

			public static void MakeCurved ( Mesh mesh, float widthToHeightRatio, float arc, float radius, int points ) {
				var arclength = arc * radius;
				var height = arclength / ( widthToHeightRatio );
				for ( var i = 0; i < points; i++ ) {
					var start = arc / points * i - arc / 2;
					var end = arc / points * ( i + 1 ) - arc / 2;

					var posA = new Vector2( MathF.Sin( end ), MathF.Cos( end ) ) * radius;
					var posB = new Vector2( MathF.Sin( start ), MathF.Cos( start ) ) * radius;

					mesh.AddQuad( new Quad(
						new Vector3( posB.X, height / 2, posB.Y ), new Vector3( posA.X, height / 2, posA.Y ),
						new Vector3( posB.X, -height / 2, posB.Y ), new Vector3( posA.X, -height / 2, posA.Y )
					), new Vector2( (float)i / points, 1 ), new Vector2( (float)( i + 1 ) / points, 1 ), new Vector2( (float)i / points, 0 ), new Vector2( (float)( i + 1 ) / points, 0 ) );
				}
			}
		}
	}
}
