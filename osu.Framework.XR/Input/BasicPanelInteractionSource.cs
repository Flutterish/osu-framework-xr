using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Physics;

namespace osu.Framework.XR.Input;

/// <summary>
/// A system which allows pointer and keyboard interaction with panels contained within a scene
/// </summary>
public partial class BasicPanelInteractionSource : Drawable {
	protected readonly PhysicsSystem Physics;
	protected readonly Scene Scene;

	public readonly Bindable<Panel?> FocusedPanelBindable = new();
	public Panel? FocusedPanel {
		get => FocusedPanelBindable.Value;
		set => FocusedPanelBindable.Value = value;
	}

	bool touchDown = false;
	public readonly BindableBool UseTouchBindable = new( false );
	public bool UseTouch {
		get => UseTouchBindable.Value;
		set => UseTouchBindable.Value = value;
	}

	PanelInteractionSystem.Source source;
	public BasicPanelInteractionSource ( Scene scene, PhysicsSystem physics, PanelInteractionSystem system ) {
		Physics = physics;
		Scene = scene;
		source = system.GetSource( this );

		FocusedPanelBindable.BindTo( source.FocusedPanelBindable );
		UseTouchBindable.BindValueChanged( v => {
			if ( v.NewValue )
				source.ReleaseMouse();
			else
				source.TouchUp();

			touchDown = false;
		} );
	}

	protected Panel? TryHit ( Vector2 e, out Vector2 pos ) {
		if ( Physics.TryHitRay( Scene.Camera.Position, Scene.Camera.DirectionOf( e, Scene.DrawWidth, Scene.DrawHeight ), out var hit ) && hit.Collider is Panel panel ) {
			pos = panel.GlobalSpaceContentPositionAt( hit.TrisIndex, hit.Point );
			return panel;
		}

		pos = default;
		return null;
	}

	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		e.Target = Scene;
		if ( UseTouch ) {
			if ( touchDown && TryHit( e.MousePosition, out var pos ) == FocusedPanel ) {
				source.TouchMove( pos );
				return true;
			}
		}
		else if ( TryHit( e.MousePosition, out var pos ) is Panel panel ) {
			panel.Content.MoveMouse( pos );
			return true;
		}

		return base.OnMouseMove( e );
	}

	protected override bool OnDragStart ( DragStartEvent e ) {
		e.Target = Scene;
		if ( TryHit( e.MouseDownPosition, out _ ) != null ) 			
			return false;

		return base.OnDragStart( e );
	}

	protected override bool OnMouseDown ( MouseDownEvent e ) {
		e.Target = Scene;
		if ( TryHit( e.MouseDownPosition, out var pos ) is Panel panel ) {
			FocusedPanel = panel;

			if ( UseTouch ) {
				touchDown = true;
				source.TouchDown( pos );
			}
			else {
				source.MoveMouse( pos );
				source.Press( e.Button );
			}

			return true;
		}
		else {
			FocusedPanel = null;
		}

		return base.OnMouseDown( e );
	}

	protected override void OnMouseUp ( MouseUpEvent e ) {
		if ( UseTouch ) {
			touchDown = false;
			source.TouchUp();
		}
		else {
			source.Release( e.Button );
		}
	}

	protected override bool OnScroll ( ScrollEvent e ) {
		e.Target = Scene;
		if ( TryHit( e.MousePosition, out _ ) is Panel panel ) {
			panel.Content.Scroll += e.ScrollDelta;
			return true;
		}

		return base.OnScroll( e );
	}

	protected override bool OnKeyDown ( KeyDownEvent e ) {
		e.Target = Scene;
		if ( FocusedPanel != null ) {
			source.Press( e.Key );
			return true;
		}

		return base.OnKeyDown( e );
	}

	protected override void OnKeyUp ( KeyUpEvent e ) {
		e.Target = Scene;
		if ( FocusedPanel != null ) {
			source.Release( e.Key );
			return;
		}

		base.OnKeyUp( e );
	}
}