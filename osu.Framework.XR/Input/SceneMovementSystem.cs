using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Input;

/// <summary>
/// A system which allows pointer and keyboard movement though a scene.
/// </summary>
public class SceneMovementSystem : Drawable {
	protected readonly Scene Scene;
	public ControlType ControlType = ControlType.Orbit;
	Vector3 cameraOrigin;
	public SceneMovementSystem ( Scene scene ) {
		Scene = scene;
		Scene.Camera.Z = -10;
	}

	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		if ( ControlType is ControlType.Fly ) {
			e.Target = Scene;

			var eulerX = Math.Clamp( e.MousePosition.Y / DrawHeight * 180 - 90, -89, 89 );
			var eulerY = e.MousePosition.X / DrawWidth * 720 + 360;

			Scene.Camera.Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, eulerY * MathF.PI / 180 )
				* Quaternion.FromAxisAngle( Vector3.UnitX, eulerX * MathF.PI / 180 );
		}

		return false;
	}

	protected override bool OnDragStart ( DragStartEvent e ) {
		return ControlType is ControlType.Orbit;
	}
	protected override void OnDrag ( DragEvent e ) {
		if ( ControlType is ControlType.Orbit ) {
			e.Target = Scene;
			var dx = e.Delta.X / Scene.DrawWidth;
			var dy = e.Delta.Y / Scene.DrawHeight;

			if ( e.ShiftPressed ) {
				var m = ( Scene.Camera.Position - cameraOrigin ).Length * 2;
				Scene.Camera.Position -= m * dx * Scene.Camera.Right;
				Scene.Camera.Position += m * dy * Scene.Camera.Up;
				cameraOrigin -= m * dx * Scene.Camera.Right;
				cameraOrigin += m * dy * Scene.Camera.Up;
			}
			else {
				var eulerX = dy * 2 * MathF.PI;
				var eulerY = dx * 4 * MathF.PI;

				var quat = Quaternion.FromAxisAngle( Vector3.UnitY, -eulerY );
				Scene.Camera.Position = cameraOrigin + quat.Inverted().Apply( Scene.Camera.Position - cameraOrigin );
				quat = Quaternion.FromAxisAngle( Scene.Camera.Right, -eulerX );
				Scene.Camera.Position = cameraOrigin + quat.Inverted().Apply( Scene.Camera.Position - cameraOrigin );

				Scene.Camera.Rotation = ( cameraOrigin - Scene.Camera.Position ).LookRotation();
			}
		}
	}

	protected override bool OnScroll ( ScrollEvent e ) {
		if ( ControlType is ControlType.Orbit ) 			Scene.Camera.Position = cameraOrigin + ( Scene.Camera.Position - cameraOrigin ) * ( 1 + e.ScrollDelta.Y / 10 );

		return base.OnScroll( e );
	}

	protected override void Update () {
		base.Update();

		if ( ControlType is ControlType.Fly ) {
			var camera = Scene.Camera;
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