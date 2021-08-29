using osu.Framework.XR.Extensions;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Testing;
using osu.Framework.XR.Testing.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Maths {
	public class TestSceneBarycentric3D : TestScene3D {
		Simplex3ShellIndicator simplex;
		PointIndicator point;
		PointIndicator indicator;

		public TestSceneBarycentric3D () {
			Add( simplex = new Simplex3ShellIndicator( Scene ) { Kind = Kind.Component } );
			Add( point = new PointIndicator( Scene ) { Kind = Kind.Control } );
			Add( indicator = new PointIndicator( Scene ) { Kind = Kind.Result, Scale = new Vector2( 0.7f ) } );

			simplex.PointA.Value = new Vector3( 1, 0, 0 );
			simplex.PointB.Value = new Vector3( 0, 0, 1 );
			simplex.PointC.Value = new Vector3( 0, 0, -1 );
			simplex.PointD.Value = new Vector3( 0, 1, 0 );

			point.Current.Value = new Vector3( 0.25f, 0.25f, 0 );

			(point.Current, simplex.PointA, simplex.PointB, simplex.PointC, simplex.PointD).BindValuesChanged( (p, a, b, c, d) => {
				var barycentric = Triangles.Barycentric( a, b, c, d, p );
				indicator.Current.Value = a * barycentric.X + b * barycentric.Y + c * barycentric.Z + d * barycentric.W;

				indicator.Colour = Triangles.IsPointInside( p, a, b, c, d ) ? Color4.Violet : Color4.Black;
			}, true );
		}
	}
}
