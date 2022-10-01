using osu.Framework.XR.Testing;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Physics.Lines;

public class TestSceneClosestPoints : BasicTestScene {
	RayIndicator rayA;
	RayIndicator rayB;
	PointIndicator pointA;
	PointIndicator pointB;

	public TestSceneClosestPoints () {
		Add( rayA = new RayIndicator( Scene ) { Kind = Kind.Component, IsBidirectional = true } );
		Add( rayB = new RayIndicator( Scene ) { Kind = Kind.Control, Tint = Color4.Orange, IsBidirectional = true } );

		Add( pointA = new PointIndicator( Scene ) { Kind = Kind.Result } );
		Add( pointB = new PointIndicator( Scene ) { Kind = Kind.Result } );

		rayA.OriginCurrent.Value = new Vector3( 1, 0, 0 );
		rayA.LookCurrent.Value = new Vector3( 1, 1, 1 );
		rayB.OriginCurrent.Value = new Vector3( 0, 1, 0 );
		rayB.LookCurrent.Value = new Vector3( 0, 1, 1 );

		(rayA.OriginCurrent, rayA.LookCurrent, rayB.OriginCurrent, rayB.LookCurrent).BindValuesChanged( ( a, _, b, _ ) => {
			(a, b) = XR.Physics.Raycast.FindClosestPointsBetween2Rays( a, rayA.Direction, b, rayB.Direction );
			pointA.Current.Value = a;
			pointB.Current.Value = b;
		}, true );
	}
}