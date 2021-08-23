using osu.Framework.XR.Components;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Rendering {
	public class CameraPositionTrackingDrawNode : CameraPositionTrackingDrawNode<Model> {
		public CameraPositionTrackingDrawNode ( Model source ) : base( source ) { }
	}

	public class CameraPositionTrackingDrawNode<T> : ModelDrawNode<T> where T : Model {
		public CameraPositionTrackingDrawNode ( T source ) : base( source ) {
			transform.Parent = source;
		}
		private Transform transform = new();
		protected override Transform Transform => transform;
		public override void Draw ( DrawSettings settings ) {
			transform.GlobalPosition = settings.GlobalCameraPos + Source.Transform.GlobalPosition;
			base.Draw( settings );
		}
	}

	public class CameraRotationTrackingDrawNode : CameraRotationTrackingDrawNode<Model> {
		public CameraRotationTrackingDrawNode ( Model source ) : base( source ) { }
	}

	public class CameraRotationTrackingDrawNode<T> : ModelDrawNode<T> where T : Model {
		public CameraRotationTrackingDrawNode ( T source ) : base( source ) {
			transform.Parent = source;
		}
		private Transform transform = new();
		protected override Transform Transform => transform;
		public override void Draw ( DrawSettings settings ) {
			transform.GlobalRotation = ( settings.GlobalCameraPos - transform.GlobalPosition ).LookRotation();
			base.Draw( settings );
		}
	}

	public class CameraTrackingDrawNode : CameraTrackingDrawNode<Model> {
		public CameraTrackingDrawNode ( Model source ) : base( source ) { }
	}

	public class CameraTrackingDrawNode<T> : ModelDrawNode<T> where T : Model {
		public CameraTrackingDrawNode ( T source ) : base( source ) {
			transform.Parent = source;
		}
		private Transform transform = new();
		protected override Transform Transform => transform;
		public override void Draw ( DrawSettings settings ) {
			transform.GlobalPosition = settings.GlobalCameraPos + Source.Transform.GlobalPosition;
			transform.GlobalRotation = ( settings.GlobalCameraPos - transform.GlobalPosition ).LookRotation();
			base.Draw( settings );
		}
	}
}
