using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;

namespace osu.Framework.XR.Graphics;
public partial class Drawable3D {
	public sealed override bool UpdateSubTreeMasking ( Drawable source, RectangleF maskingBounds )
		=> false;

	public sealed override DrawColourInfo DrawColourInfo => default;
	public sealed override DrawInfo DrawInfo => default;
	protected sealed override RectangleF ComputeChildMaskingBounds ( RectangleF maskingBounds )
		=> default;

	public sealed override Quad ScreenSpaceDrawQuad => default;

	protected sealed override void UpdateAfterAutoSize () { }
	protected sealed override bool ReceivePositionalInputAtSubTree ( Vector2 screenSpacePos )
		=> false;

	public sealed override bool ReceivePositionalInputAt ( Vector2 screenSpacePos )
		=> false;

	public sealed override bool PropagatePositionalInputSubTree => false;

	protected sealed override bool ComputeIsMaskedAway ( RectangleF maskingBounds )
		=> false;

	public sealed override bool HandlePositionalInput => false;

	public sealed override bool Contains ( Vector2 screenSpacePos )
		=> false;
}
