using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.VirtualReality;

/// <summary>
/// Anchor point for <see cref="VrCompositor"/> to decide where to position the player.
/// This is positioned at the players feet
/// </summary>
public partial class VrPlayer : CompositeDrawable3D {
	[Resolved]
	public VrCompositor Compositor { get; private set; } = null!;

	public ulong RenderMask = ulong.MaxValue;

	public Vector3 PositionOffset;
	public Quaternion RotationOffset = Quaternion.Identity;

	public Vector3 InGlobalSpace ( Vector3 local ) {
		return RotationOffset.Apply( local ) + PositionOffset;
	}
	public Quaternion InGlobalSpace ( Quaternion local ) {
		return RotationOffset * local;
	}

	protected override void LoadComplete () {
		Compositor.RegisterPlayer( this );

		base.LoadComplete();
	}
}
