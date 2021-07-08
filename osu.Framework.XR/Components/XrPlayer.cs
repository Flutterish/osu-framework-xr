using OpenVR.NET;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.XR.GameHosts;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Projection;
using osuTK;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			Add( Camera );
		}
	}
}
