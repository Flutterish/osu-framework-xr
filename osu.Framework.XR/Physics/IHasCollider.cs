using osu.Framework.XR.Graphics;

namespace osu.Framework.XR.Physics {
	public interface IHasCollider {
		Mesh Mesh { get; }
		bool IsColliderEnabled { get; }
	}
}
