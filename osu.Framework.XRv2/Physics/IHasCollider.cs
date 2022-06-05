using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;

namespace osu.Framework.XR.Physics;

public interface IHasCollider : IHasMatrix {
	ITriangleMesh Mesh { get; }
	bool IsColliderEnabled { get; }
	/// <summary>
	/// Physics layer as a bitfield
	/// </summary>
	ulong PhysicsLayer { get; }
}