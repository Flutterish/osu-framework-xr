using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Physics;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Panels;

public class TestScenePanelInput : BasicTestScene, IRequireHighFrequencyMousePosition {
	Panel panel;
	public TestScenePanelInput () {
		Scene.Add( panel = new Panel {
			ContentAutoSizeAxes = Axes.Both
		} );

		panel.Content.Add( new Box { Size = new( 500 ), Colour = Color4.Gray } );
		panel.Content.Add( new SpriteText { Text = "Hello, World!" } );
		panel.Content.Add( new CursorContainer { RelativeSizeAxes = Axes.Both } );
	}

	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		e.Target = Scene;
		if ( Raycast.TryHit( Scene.Camera.Position, Scene.Camera.DirectionOf( e.MousePosition, Scene.DrawWidth, Scene.DrawHeight ), panel.Mesh, panel.Matrix, out var hit ) ) {
			var pos = panel.ContentPositionAt( hit.TrisIndex, hit.Point );
			panel.Content.MoveMouse( pos );

			return true;
		}

		return false;
	}
}
