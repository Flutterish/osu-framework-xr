using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.XR.Components;

namespace osu.Framework.XR.Testing {
	public abstract class TestScene3D : TestScene {
		protected readonly Scene Scene;

		public TestScene3D () {
			Add( Scene = new TestingScene() );
		}

		public override void Add ( Drawable drawable ) {
			if ( drawable is Drawable3D d3 )
				Scene.Add( d3 );
			else
				base.Add( drawable );
		}
	}
}
