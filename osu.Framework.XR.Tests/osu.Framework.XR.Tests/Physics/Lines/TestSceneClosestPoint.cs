using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.Framework.XR.Extensions;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Tests.Components;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Lines {
	public class TestSceneClosestPoint : TestScene3D {
		Bindable<Vector3> from = new( new Vector3( -1, 0, 0 ) );
		Bindable<Vector3> to = new( new Vector3( 1, 0, 0 ) );
		Bindable<Vector3> closest = new();
		Bindable<Vector3> other = new( new Vector3( 0.5f, 1, 0 ) );

		Path3D line;

		public TestSceneClosestPoint () {
			AddRange( new Drawable[] {
				new PointIndicator( Scene ) { Current = closest, Colour = Colour4.Violet, Scale = new Vector2( 1.5f ), AllowDragging = false },
				new PointIndicator( Scene ) { Current = from, Colour = Colour4.Blue },
				new PointIndicator( Scene ) { Current = to, Colour = Colour4.Blue },
				new PointIndicator( Scene ) { Current = other, Colour = Colour4.Red },
				line = new Path3D { Tint = Colour4.Green }
			} );

			(from, to).BindValuesChanged( (a,b) => {
				line.ClearNodes();
				line.AddNode( a );
				line.AddNode( b );
			}, true );

			(from, to, other).BindValuesChanged( (a,b,c) => {
				closest.Value = Raycast.ClosestPoint( a, b, c );
			}, true );
		}
	}
}
