using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Rendering;
using osuTK;
using System;

namespace osu.Framework.XR.Tests;

public class SampleTestScene : TestScene {
	protected readonly Scene Scene;
	public SampleTestScene () {
		Add( Scene = new Scene { RelativeSizeAxes = Framework.Graphics.Axes.Both } );
	}

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );
		var store = parent.Get<Game>().Resources;
		var materials = new MaterialStore( new NamespacedResourceStore<byte[]>( store, "Resources/Shaders" ) );
		var textures = new TextureStore(
			parent.Get<GameHost>().CreateTextureLoaderStore( new NamespacedResourceStore<byte[]>( store, "Resources/Textures" ) ),
			useAtlas: true,
			osuTK.Graphics.ES30.All.Nearest,
			manualMipmaps: false,
			scaleAdjust: 1
		);
		deps.Cache( materials );
		deps.Cache( textures );
		return base.CreateChildDependencies( deps );
	}

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

		if ( dir != Vector3.Zero )
			camera.Position += dir.Normalized() * (float)Time.Elapsed / 300;
	}
}
