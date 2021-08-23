using osu.Framework.XR.Projection;
using osuTK;

namespace osu.Framework.XR.Components {
	/// <summary>
	/// A container which tracks the XR user's position and orientation.
	/// </summary>
	public class XrPlayer : CompositeDrawable3D {
		/// <summary>
		/// Offset from the actual 3D position the user is at.
		/// </summary>
		public Vector3 PositionOffset;
		public Camera Camera { get; } = new();

		public XrPlayer () {
			AddInternal( Camera );
		}
	}
}
