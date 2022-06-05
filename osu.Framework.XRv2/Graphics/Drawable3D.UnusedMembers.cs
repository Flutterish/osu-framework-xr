using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;

namespace osu.Framework.XR.Graphics;
public partial class Drawable3D {
	public sealed override bool UpdateSubTreeMasking ( Drawable source, RectangleF maskingBounds ) {
		return false;
	}

	protected override void UpdateAfterAutoSize () { }
	protected override bool ReceivePositionalInputAtSubTree ( Vector2 screenSpacePos )
		=> false;

	public override bool ReceivePositionalInputAt ( Vector2 screenSpacePos )
		=> false;

	public sealed override bool PropagatePositionalInputSubTree => false;

	protected sealed override bool ComputeIsMaskedAway ( RectangleF maskingBounds )
		=> false;

	public sealed override bool HandlePositionalInput => false;

	public override bool Contains ( Vector2 screenSpacePos )
		=> false;
}
