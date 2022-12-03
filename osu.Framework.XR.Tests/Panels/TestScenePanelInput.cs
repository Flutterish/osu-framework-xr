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

public partial class TestScenePanelInput : BasicTestScene, IRequireHighFrequencyMousePosition {
	bool useTouch;
	protected readonly Panel Panel;
	public TestScenePanelInput () {
		Scene.Add( Panel = new Panel {
			ContentAutoSizeAxes = Axes.Both
		} );

		Panel.Content.HasFocus = true;

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

		AddToggleStep( "Use Touch", v => {
			Panel.Content.ReleaseAllInput();
			useTouch = v;
			touchDown = false;
		} );
	}

	bool tryHit ( Vector2 e, out Vector2 pos ) {
		RaycastHit hit = new();
		if ( Raycast.TryHit( Scene.Camera.Position, Scene.Camera.DirectionOf( e, Scene.DrawWidth, Scene.DrawHeight ), Panel, ref hit ) ) {
			pos = Panel.GlobalSpaceContentPositionAt( hit.TrisIndex, hit.Point );

			return true;
		}
		pos = default;
		return false;
	}

	bool touchDown = false;
	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out var pos ) ) {
			if ( useTouch && touchDown )
				Panel.Content.TouchMove( this, pos );
			else if ( !useTouch )
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
			if ( useTouch ) {
				touchDown = true;
				Panel.Content.TouchDown( this, pos );
			}
			else {
				Panel.Content.MoveMouse( pos );
				Panel.Content.Press( e.Button );
			}
			
			return true;
		}
		else {
			Panel.Content.ReleaseAllInput();
		}

		return base.OnMouseDown( e );
	}

	protected override void OnMouseUp ( MouseUpEvent e ) {
		if ( useTouch ) {
			touchDown = false;
			Panel.Content.TouchUp( this );
		}
		else
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
