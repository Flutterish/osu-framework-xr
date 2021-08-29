using osu.Framework.XR.Extensions;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Testing;
using osu.Framework.XR.Testing.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Maths {
	public class TestSceneBarycentric2D : TestScene3D {
		TriangleIndicator triangle;
		PointIndicator point;
		PointIndicator indicator;

		public TestSceneBarycentric2D () {
			Add( triangle = new TriangleIndicator( Scene ) { Kind = Kind.Component } );
			Add( point = new PointIndicator( Scene ) { Kind = Kind.Control } );
			Add( indicator = new PointIndicator( Scene ) { Kind = Kind.Result } );

			triangle.PointA.Value = new Vector3( 1, 0, 0 );
			triangle.PointB.Value = new Vector3( -1, 0, 0 );
			triangle.PointC.Value = new Vector3( 0, 0, 1 );

			point.Current.Value = new Vector3( 0, 1, 0 );

			(point.Current, triangle.PointA, triangle.PointB, triangle.PointC).BindValuesChanged( (p, a, b, c) => {
				var barycentric = Triangles.Barycentric( a, b, c, p );

				indicator.Current.Value = barycentric.X * a + barycentric.Y * b + barycentric.Z * c;
			}, true );
		}
	}
}
