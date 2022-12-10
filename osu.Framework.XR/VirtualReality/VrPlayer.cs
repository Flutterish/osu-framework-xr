using osu.Framework.XR.Graphics;

namespace osu.Framework.XR.VirtualReality;

/// <summary>
/// Anchor point for <see cref="VrCompositor"/> to decide where to position the player.
/// This is positioned at the players feet
/// </summary>
public partial class VrPlayer : CompositeDrawable3D {
	[Resolved]
	public VrCompositor Compositor { get; private set; } = null!;

	protected override void LoadComplete () {
		Compositor.RegisterPlayer( this );

		base.LoadComplete();
	}
}
