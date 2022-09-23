using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Panels;

public class TestSceneDefaultPanel : BasicTestScene {
	public TestSceneDefaultPanel () {
		Panel panel;
		Scene.Add( panel = new Panel {
			ContentAutoSizeAxes = Axes.Both
		} );

		Box rotatingBox;
		panel.Content.Add( new Box { Size = new( 500 ), Colour = Color4.Green } );
		panel.Content.Add( new Box { Size = new( 250 ), Colour = Color4.Red } );
		panel.Content.Add( new Box { Position = new( 250 ), Size = new( 250 ), Colour = Color4.Blue } );
		panel.Content.Add( rotatingBox = new Box { Origin = Anchor.Centre, Anchor = Anchor.Centre, Size = new( 100 ) } );
		rotatingBox.OnUpdate += d => {
			d.Rotation = (float)d.Time.Current / 10;
		};
	}
}
