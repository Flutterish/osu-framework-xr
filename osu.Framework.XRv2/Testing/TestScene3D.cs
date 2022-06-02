using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Framework.XR.Graphics.Rendering;

namespace osu.Framework.XR.Testing;

public abstract class TestScene3D : TestScene {
	protected readonly Scene Scene;
	public TestScene3D () {
		Add( Scene = CreateScene() );
		Scene.RelativeSizeAxes = Framework.Graphics.Axes.Both;
	}

	protected virtual Scene CreateScene ()
		=> new();

	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		e.Target = Scene;

		var eulerX = Math.Clamp( e.MousePosition.Y / DrawHeight * 180 - 90, -89, 89 );
		var eulerY = e.MousePosition.X / DrawWidth * 720 + 360;

		Scene.Camera.Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, eulerY * MathF.PI / 180 )
			* Quaternion.FromAxisAngle( Vector3.UnitX, eulerX * MathF.PI / 180 );

		return false;
	}

	protected override void Update () {
		base.Update();

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
