using osu.Framework.XR.Physics;

namespace osu.Framework.XR.Components {
	public class Collider : Model, IHasCollider {
		public bool IsColliderEnabled { get; set; } = true;
	}
}
