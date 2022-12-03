using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Raycast;

public partial class TestSceneIntersectPlane : BasicTestScene {
	PlaneIndicator plane;
	RayIndicator ray;
	PointIndicator hit;

	public TestSceneIntersectPlane () {
		Add( plane = new PlaneIndicator( Scene ) { Kind = Kind.Component } );
		Add( ray = new RayIndicator( Scene ) { Kind = Kind.Control } );
		Add( hit = new PointIndicator( Scene ) { Kind = Kind.Result } );

		plane.LookCurrent.Value = Vector3.UnitY;
		ray.OriginCurrent.Value = new Vector3( 1, 1, 0 );
		ray.LookCurrent.Value = -Vector3.UnitY;

		(plane.OriginCurrent, plane.LookCurrent, ray.OriginCurrent, ray.LookCurrent, ray.IsBidirectionalBindable).BindValuesChanged( ( planePoint, _, rayOrigin, _, bi ) => {
			RaycastHit raycastHit = new();
			if ( XR.Physics.Raycast.TryHit( rayOrigin, ray.Direction, planePoint, plane.Normal, ref raycastHit, bi ) ) {
				hit.Current.Value = raycastHit.Point;
				hit.Alpha = 1;
			}
			else {
				hit.Alpha = 0;
			}
		}, true );
	}
}