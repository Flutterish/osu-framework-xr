using osu.Framework.Graphics;
using osu.Framework.XR.Extensions;
using osu.Framework.XR.Testing;
using osu.Framework.XR.Testing.Components;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Lines {
	public class TestSceneClosestPoint : TestScene3D {
		LineIndicator line;
		PointIndicator point;

		PointIndicator closest;
		DashedLineVisual dash;

		public TestSceneClosestPoint () {
			Add( line = new LineIndicator( Scene ) { Colour = Colour4.Blue, Tint = Colour4.Cyan } );
			Add( point = new PointIndicator( Scene ) { Colour = Colour4.Red } );
			Add( closest = new PointIndicator( Scene ) { Colour = Colour4.Violet, AllowDragging = false } );
			Add( dash = new DashedLineVisual( Scene ) { Colour = Colour4.Violet } );

			line.PointA.Value = new Vector3( -1, 0, 0 );
			line.PointB.Value = new Vector3( 1, 0, 0 );

			point.Current.Value = new Vector3( 0, 1, 1 );

			dash.PointA = closest.Current;
			dash.PointB = point.Current;

			(line.PointA, line.PointB, point.Current).BindValuesChanged( (a,b,c) => {
				closest.Current.Value = XR.Physics.Raycast.ClosestPoint( a, b, c );
			}, true );
		}
	}
}
