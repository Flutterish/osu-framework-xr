using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Framework.XR.Testing;

public class TestBrowser : Game {
	[BackgroundDependencyLoader]
	private void load () {
		var store = new DllResourceStore( XR.Resources.ResourceAssembly );
		Resources.AddStore( store );

		Child = new SafeAreaContainer {
			RelativeSizeAxes = Axes.Both,
			Child = new DrawSizePreservingFillContainer {
				Children = new Drawable[]
				{
					new osu.Framework.Testing.TestBrowser(),
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