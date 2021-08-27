using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Framework.XR.Components;
using osu.Framework.XR.Tests.Components;
using osuTK;

namespace osu.Framework.XR.Tests {
	public abstract class TestScene3D : TestScene {
		protected readonly Scene Scene;
		private Drawable3D cameraYRotContainer;
		private Drawable3D cameraXRotContainer;

		public TestScene3D () {
			Scene = new Scene {
				RelativeSizeAxes = Axes.Both,
				Camera = new() { Position = new Vector3( 0, 0, -3 ) }
			};
			Scene.Add( cameraYRotContainer = new Container3D {
				Child = cameraXRotContainer = new Container3D {
					Child = Scene.Camera,
					Rotation = Quaternion.FromAxisAngle( Vector3.UnitX, 0.4f )
				},
				Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, 3.5f )
			} );
			Scene.Add( new AxisVisual() );
			Add( Scene );
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Scene.RenderToScreen = true;
		}

		public override void Add ( Drawable drawable ) {
			if ( drawable is Drawable3D d3 )
				Scene.Add( d3 );
			else
				base.Add( drawable );
		}

		Quaternion startYRot;
		Quaternion startXRot;
		protected override bool OnDragStart ( DragStartEvent e ) {
			startYRot = cameraYRotContainer.Rotation;
			startXRot = cameraXRotContainer.Rotation;

			return true;
		}

		protected override void OnDrag ( DragEvent e ) {
			var delta = e.MousePosition - e.MouseDownPosition;
			cameraYRotContainer.RotateTo( Quaternion.FromAxisAngle( Vector3.UnitY, delta.X / 100 ) * startYRot, 200, Easing.Out );
			cameraXRotContainer.RotateTo( Quaternion.FromAxisAngle( Vector3.UnitX, delta.Y / 100 ) * startXRot, 200, Easing.Out );
		}
	}
}
