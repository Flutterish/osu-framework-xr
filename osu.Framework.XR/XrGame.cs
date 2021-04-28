using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.XR.Components;

namespace osu.Framework.XR {
	public abstract class XrGame : Game {
		public Scene Scene { get; protected set; }
		public abstract Manifest XrManifest { get; }
	}
}
