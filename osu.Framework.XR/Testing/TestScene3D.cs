using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Input;

namespace osu.Framework.XR.Testing;

public abstract class TestScene3D : TestScene {
	protected readonly Scene Scene;
	protected readonly SceneMovementSystem MovementSystem;

	public ControlType ControlType {
		get => MovementSystem.ControlType;
		set => MovementSystem.ControlType = value;
	}

	public TestScene3D () {
		Add( Scene = CreateScene() );
		Scene.RelativeSizeAxes = Framework.Graphics.Axes.Both;

		Add( MovementSystem = new SceneMovementSystem( Scene ) );
	}

	protected override bool Handle ( UIEvent e ) {
		return base.Handle( e ) || MovementSystem.TriggerEvent( e );
	}

	protected virtual Scene CreateScene ()
		=> new();
}