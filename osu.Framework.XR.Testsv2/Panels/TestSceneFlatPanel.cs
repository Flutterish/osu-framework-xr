using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Panels;

public class TestSceneFlatPanel : BasicTestScene {
	public TestSceneFlatPanel () {
		Panel panel;
		Scene.Add( panel = new Panel {
			ContentAutoSizeAxes = Axes.Both
		} );

		panel.Content.Add( new Box { Size = new( 500 ), Colour = Color4.Green } );
		panel.Content.Add( new Box { Size = new( 250 ), Colour = Color4.Red } );
		panel.Content.Add( new Box { Position = new( 250 ), Size = new( 250 ), Colour = Color4.Blue } );
	}
}
