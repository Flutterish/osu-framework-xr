using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Physics;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Framework.XR.Tests.Panels;

public class TestScenePanelInput : BasicTestScene, IRequireHighFrequencyMousePosition {
	protected readonly Panel Panel;
	public TestScenePanelInput () {
		Scene.Add( Panel = new Panel {
			ContentAutoSizeAxes = Axes.Both
		} );

		BasicDropdown<FillDirection> dropdown;
		Panel.Content.Add( new Box { Size = new( 500 ), Colour = Color4.Gray } );
		Panel.Content.Add( new FillFlowContainer {
			Direction = FillDirection.Vertical,
			Children = new Drawable[] {
				new SpriteText { Text = "Hello, World!" },
				dropdown = new BasicDropdown<FillDirection> { Width = 100 },
				new BasicTextBox { Width = 100, Height = 20 }
			}
		} );
		Panel.Content.Add( new CursorContainer { RelativeSizeAxes = Axes.Both } );
		foreach ( var i in Enum.GetValues<FillDirection>() )
			dropdown.AddDropdownItem( i );
	}

	bool tryHit ( Vector2 e, out Vector2 pos ) {
		if ( Raycast.TryHit( Scene.Camera.Position, Scene.Camera.DirectionOf( e, Scene.DrawWidth, Scene.DrawHeight ), Panel.Mesh, Panel.Matrix, out var hit ) ) {
			pos = Panel.ContentPositionAt( hit.TrisIndex, hit.Point );

			return true;
		}
		pos = default;
		return false;
	}

	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out var pos ) ) {
			Panel.Content.MoveMouse( pos );

			return true;
		}

		return base.OnMouseMove( e );
	}
	protected override bool OnDragStart ( DragStartEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MouseDownPosition, out _ ) ) {
			return false;
		}

		return base.OnDragStart( e );
	}

	protected override bool OnMouseDown ( MouseDownEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MouseDownPosition, out var pos ) ) {
			Panel.Content.MoveMouse( pos );
			Panel.Content.Press( e.Button );
			return true;
		}
		else {
			Panel.Content.ReleaseAllInput();
		}

		return base.OnMouseDown( e );
	}

	protected override void OnMouseUp ( MouseUpEvent e ) {
		Panel.Content.Release( e.Button );

		if ( !tryHit( e.MouseDownPosition, out _ ) ) {
			Panel.Content.ReleaseAllInput();
		}
	}

	protected override bool OnScroll ( ScrollEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out _ ) ) {
			Panel.Content.Scroll += e.ScrollDelta;
			return true;
		}

		return base.OnScroll( e );
	}

	protected override bool OnKeyDown ( KeyDownEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out _ ) ) {
			Panel.Content.Press( e.Key );
			return true;
		}

		return base.OnKeyDown( e );
	}

	protected override void OnKeyUp ( KeyUpEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out _ ) ) {
			Panel.Content.Release( e.Key );
			return;
		}

		base.OnKeyUp( e );
	}
}
