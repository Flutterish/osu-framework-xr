using osu.Framework.XR.Extensions;
using osu.Framework.XR.Testing;
using osu.Framework.XR.Testing.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Physics.Lines {
	public class TestSceneClosestPoints : TestScene3D {
		RayIndicator rayA;
		RayIndicator rayB;
		PointIndicator pointA;
		PointIndicator pointB;

		public TestSceneClosestPoints () {
			Add( rayA = new RayIndicator( Scene ) { Colour = Color4.Blue, Tint = Color4.Cyan, IsBidirectional = true } );
			Add( rayB = new RayIndicator( Scene ) { Colour = Color4.Red, Tint = Color4.Orange, IsBidirectional = true } );

			Add( pointA = new PointIndicator( Scene ) { Colour = Color4.Violet, AllowDragging = false } );
			Add( pointB = new PointIndicator( Scene ) { Colour = Color4.Violet, AllowDragging = false } );

			rayA.OriginCurrent.Value = new Vector3( 1, 0, 0 );
			rayA.LookCurrent.Value = new Vector3( 1, 1, 1 );
			rayB.OriginCurrent.Value = new Vector3( 0, 1, 0 );
			rayB.LookCurrent.Value = new Vector3( 0, 1, 1 );

			(rayA.OriginCurrent, rayA.LookCurrent, rayB.OriginCurrent, rayB.LookCurrent).BindValuesChanged( (a, _, b, _) => {
				(a,b) = XR.Physics.Raycast.FindClosestPointsBetween2Rays( a, rayA.Direction, b, rayB.Direction );
				pointA.Current.Value = a;
				pointB.Current.Value = b;
			}, true );
		}
	}
}
