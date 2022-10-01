using osu.Framework.XR.Testing;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Physics.Raycast;

public class TestSceneIntersectRays : BasicTestScene {
	RayIndicator rayA;
	RayIndicator rayB;
	PointIndicator point;

	public TestSceneIntersectRays () {
		Add( rayA = new RayIndicator( Scene ) { Kind = Kind.Component, IsBidirectional = true } );
		Add( rayB = new RayIndicator( Scene ) { Kind = Kind.Control, IsBidirectional = true } );

		Add( point = new PointIndicator( Scene ) { Colour = Color4.Violet, AllowDragging = false } );

		rayA.OriginCurrent.Value = new Vector3( 1, 0, 0 );
		rayA.LookCurrent.Value = new Vector3( 1, 1, 1 );
		rayB.OriginCurrent.Value = new Vector3( 0, 1, 0 );
		rayB.LookCurrent.Value = new Vector3( 0, 1, 1 );

		(rayA.OriginCurrent, rayA.LookCurrent, rayB.OriginCurrent, rayB.LookCurrent).BindValuesChanged( ( a, _, b, _ ) => {
			if ( XR.Physics.Raycast.TryHitRay( a, rayA.Direction, b, rayB.Direction, out var hit, 0.01f ) ) {
				point.Alpha = 1;
				point.Current.Value = hit;
			}
			else {
				point.Alpha = 0;
			}
		}, true );
	}
}