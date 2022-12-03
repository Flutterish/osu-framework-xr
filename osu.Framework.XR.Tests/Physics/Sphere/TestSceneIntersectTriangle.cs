using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Sphere;

public partial class TestSceneIntersectTriangle : BasicTestScene {
	TriangleIndicator triangle;
	SphereShellIndicator sphere;
	PointIndicator hit;

	public TestSceneIntersectTriangle () {
		Add( triangle = new TriangleIndicator( Scene ) { Kind = Kind.Component } );
		Add( sphere = new SphereShellIndicator( Scene ) { Kind = Kind.Control } );
		Add( hit = new PointIndicator( Scene ) { Kind = Kind.Result } );

		triangle.PointA.Value = new Vector3( 1, 0, 0 );
		triangle.PointB.Value = new Vector3( -1, 0, 0 );
		triangle.PointC.Value = new Vector3( 0, 2, 0 );
		sphere.Current.Value = new Vector3( 0, 0, 1 );

		(triangle.PointA, triangle.PointB, triangle.PointC, sphere.Current, sphere.RadiusBindable).BindValuesChanged( ( _, _, _, origin, radius ) => {
			SphereHit sphereHit = new();
			if ( XR.Physics.Sphere.TryHit( origin, radius, triangle.Face, ref sphereHit ) ) {
				hit.Current.Value = sphereHit.Point;
				hit.Alpha = 1;
			}
			else {
				hit.Alpha = 0;
			}
		}, true );
	}
}