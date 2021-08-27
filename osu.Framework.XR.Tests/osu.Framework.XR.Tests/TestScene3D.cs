using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.XR.Components;
using osu.Framework.XR.Tests.Components;
using osuTK;

namespace osu.Framework.XR.Tests {
	public abstract class TestScene3D : TestScene {
		protected readonly Scene Scene;

		public TestScene3D () {
			Scene = new Scene {
				RelativeSizeAxes = Axes.Both,
				Camera = new() { Position = new Vector3( 0, 0, -3 ) }
			};
			Scene.Add( Scene.Camera );
			Add( Scene );
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Scene.RenderToScreen = true;
		}

		public override void Add ( Drawable drawable ) {
			if ( drawable is Drawable3D d3 )
				Scene.Add( d3 );
			else
				base.Add( drawable );
		}
	}
}
