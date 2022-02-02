using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;

namespace osu.Framework.XR.Testing {
	public class TestBrowser3D : Game3D {
		[BackgroundDependencyLoader]
		private void load () {
			Resources.AddStore( new DllResourceStore( XR.Resources.ResourceAssembly ) );

			Child = new SafeAreaContainer {
				RelativeSizeAxes = Axes.Both,
				Child = new DrawSizePreservingFillContainer {
					Children = new Drawable[]
					{
						new TestBrowser(),
						new CursorContainer(),
					},
				}
			};
		}

		public override void SetHost ( GameHost host ) {
			base.SetHost( host );
			host.Window.CursorState |= CursorState.Hidden;
		}
	}
}
