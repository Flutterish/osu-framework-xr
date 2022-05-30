using osu.Framework.XR.Extensions;
using osu.Framework.XR.Testing;
using osu.Framework.XR.Testing.Components;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Raycast {
	public class TestSceneIntersectTriangle : TestScene3D {
		TriangleIndicator triangle;
		RayIndicator ray;
		PointIndicator hit;

		public TestSceneIntersectTriangle () {
			Add( triangle = new TriangleIndicator( Scene ) { Kind = Kind.Component } );
			Add( ray = new RayIndicator( Scene ) { Kind = Kind.Control } );
			Add( hit = new PointIndicator( Scene ) { Kind = Kind.Result } );

			triangle.PointA.Value = new Vector3( 1, 0, 0 );
			triangle.PointB.Value = new Vector3( -1, 0, 0 );
			triangle.PointC.Value = new Vector3( 0, 2, 0 );
			ray.OriginCurrent.Value = new Vector3( 1, 0, 1 );
			ray.LookCurrent.Value = -Vector3.UnitY;

			(triangle.PointA, triangle.PointB, triangle.PointC, ray.OriginCurrent, ray.LookCurrent, ray.IsBidirectionalBindable).BindValuesChanged( (_, _, _, rayOrigin, _, bi ) => {
				if ( XR.Physics.Raycast.TryHit( rayOrigin, ray.Direction, triangle.Face, out var raycastHit, bi ) ) {
					hit.Current.Value = raycastHit.Point;
					hit.Alpha = 1;
				}
				else {
					hit.Alpha = 0;
				}
			}, true );
		}
	}
}
