using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.VirtualReality;

/// <summary>
/// Anchor point for <see cref="VrCompositor"/> to decide where to position the player.
/// This is positioned at the players feet.
/// </summary>
public partial class VrPlayer : CompositeDrawable3D {
	[Resolved]
	public VrCompositor Compositor { get; private set; } = null!;

	public ulong RenderMask = ulong.MaxValue;

	public Vector3 PositionOffset;
	public Quaternion RotationOffset = Quaternion.Identity;

	/// <summary>
	/// Applies local offsets to the value, so that values relative to the player are translated to global space.
	/// </summary>
	public Vector3 ToGlobalSpace ( Vector3 local ) {
		return RotationOffset.Apply( local ) + PositionOffset;
	}

	/// <summary>
	/// Applies local offsets to the value, so that values relative to the player are translated to global space.
	/// </summary>
	public Quaternion ToGlobalSpace ( Quaternion local ) {
		return RotationOffset * local;
	}

	protected override void LoadComplete () {
		Compositor.RegisterPlayer( this );

		base.LoadComplete();
	}
}
