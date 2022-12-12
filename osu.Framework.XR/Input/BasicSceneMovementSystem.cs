using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Input;

/// <summary>
/// A system which allows pointer and keyboard movement though a scene.
/// </summary>
public partial class BasicSceneMovementSystem : Drawable {
	protected readonly Scene Target;
	public ControlType ControlType = ControlType.Orbit;
	Vector3 cameraOrigin;
	public BasicSceneMovementSystem ( Scene target ) {
		Target = target;
		if ( Target.Camera.Position == Vector3.Zero )
			Target.Camera.Z = -10;
	}

	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		if ( ControlType is ControlType.Fly ) {
			e.Target = Target;

			var eulerX = Math.Clamp( e.MousePosition.Y / DrawHeight * 180 - 90, -89, 89 );
			var eulerY = e.MousePosition.X / DrawWidth * 720 + 360;

			Target.Camera.Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, eulerY * MathF.PI / 180 )
				* Quaternion.FromAxisAngle( Vector3.UnitX, eulerX * MathF.PI / 180 );
		}

		return false;
	}

	protected override bool OnDragStart ( DragStartEvent e ) {
		return ControlType is ControlType.Orbit;
	}
	protected override void OnDrag ( DragEvent e ) {
		if ( ControlType is ControlType.Orbit ) {
			e.Target = Target;
			var dx = e.Delta.X / Target.DrawWidth;
			var dy = e.Delta.Y / Target.DrawHeight;

			if ( e.ShiftPressed ) {
				var m = ( Target.Camera.Position - cameraOrigin ).Length * 2;
				Target.Camera.Position -= m * dx * Target.Camera.Right;
				Target.Camera.Position += m * dy * Target.Camera.Up;
				cameraOrigin -= m * dx * Target.Camera.Right;
				cameraOrigin += m * dy * Target.Camera.Up;
			}
			else {
				var eulerX = dy * 2 * MathF.PI;
				var eulerY = dx * 4 * MathF.PI;

				var quat = Quaternion.FromAxisAngle( Vector3.UnitY, -eulerY );
				Target.Camera.Position = cameraOrigin + quat.Inverted().Apply( Target.Camera.Position - cameraOrigin );
				quat = Quaternion.FromAxisAngle( Target.Camera.Right, -eulerX );
				Target.Camera.Position = cameraOrigin + quat.Inverted().Apply( Target.Camera.Position - cameraOrigin );

				Target.Camera.Rotation = ( cameraOrigin - Target.Camera.Position ).LookRotation();
			}
		}
	}

	protected override bool OnScroll ( ScrollEvent e ) {
		if ( ControlType is ControlType.Orbit ) 
			Target.Camera.Position = cameraOrigin + ( Target.Camera.Position - cameraOrigin ) * ( 1 + e.ScrollDelta.Y / 10 );

		return base.OnScroll( e );
	}

	protected override void Update () {
		base.Update();

		if ( ControlType is ControlType.Fly ) {
			var camera = Target.Camera;
			var state = GetContainingInputManager().CurrentState;
			var keyboard = state.Keyboard;

			Vector3 dir = Vector3.Zero;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.W ) )
				dir += camera.Forward;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.S ) )
				dir += camera.Back;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.A ) )
				dir += camera.Left;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.D ) )
				dir += camera.Right;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.Space ) )
				dir += camera.Up;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.ControlLeft ) )
				dir += camera.Down;

			if ( dir.Length > 0.1f )
				camera.Position += dir.Normalized() * (float)Time.Elapsed / 300;
		}
	}
}

public enum ControlType {
	Fly,
	Orbit
}