using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.XR.Components;
using osu.Framework.XR.Testing.Components;
using osuTK;
using System;

namespace osu.Framework.XR.Testing {
	/// <summary>
	/// A 3D scene with coordinate axis. Drag to rotate, shift + drag to move, scroll to zoom.
	/// </summary>
	public class TestingScene : Scene, IRequireHighFrequencyMousePosition {
		private Drawable3D cameraYRotContainer;
		private Drawable3D cameraXRotContainer;

		public TestingScene () {
			RelativeSizeAxes = Axes.Both;
			Camera = new() { Position = new Vector3( 0, 0, -3 ) };

			Add( cameraYRotContainer = new Container3D {
				Child = cameraXRotContainer = new Container3D {
					Child = Camera,
					Rotation = Quaternion.FromAxisAngle( Vector3.UnitX, 0.4f )
				},
				Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, 3.5f )
			} );
			Add( new AxisVisual() );
		}

		[BackgroundDependencyLoader]
		private void load () {
			RenderToScreen = true;
		}

		Quaternion startYRot;
		Quaternion startXRot;
		bool move;
		protected override bool OnDragStart ( DragStartEvent e ) {
			startYRot = cameraYRotContainer.Rotation;
			startXRot = cameraXRotContainer.Rotation;

			move = e.ShiftPressed;

			return true;
		}

		protected override void OnDrag ( DragEvent e ) {
			if ( move ) {
				cameraYRotContainer.Position +=
					e.Delta.X * Camera.GlobalLeft / 100 * scale +
					e.Delta.Y * Camera.GlobalUp / 100 * scale;
			}
			else {
				var delta = e.MousePosition - e.MouseDownPosition;

				cameraYRotContainer.RotateTo( Quaternion.FromAxisAngle( Vector3.UnitY, delta.X / 100 ) * startYRot, 200, Easing.Out );
				cameraXRotContainer.RotateTo( Quaternion.FromAxisAngle( Vector3.UnitX, delta.Y / 100 ) * startXRot, 200, Easing.Out );
			}
		}

		float totalZoom = 0;
		float scale => MathF.Pow( 2, totalZoom / 10 );
		protected override bool OnScroll ( ScrollEvent e ) {
			totalZoom += e.ScrollDelta.Y;
			cameraXRotContainer.ScaleTo( scale, 200, Easing.Out );
			return true;
		}
	}
}
