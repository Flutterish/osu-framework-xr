using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
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

	protected override bool OnKeyDown ( KeyDownEvent e ) {
		if ( e.Key is osuTK.Input.Key.Tilde ) {
			Logger.Log( "Forcing GC collection..." );
			GC.Collect();
		}

		return base.OnKeyDown( e );
	}
}