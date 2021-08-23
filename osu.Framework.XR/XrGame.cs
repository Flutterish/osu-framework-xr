using OpenVR.NET.Manifests;
using osu.Framework.XR.Components;
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.XR {
	public abstract class XrGame : Game {
		[MaybeNull, NotNull]
		public Scene Scene { get; protected set; }
		[MaybeNull, NotNull]
		public XrPlayer Player { get; protected set; }
		public abstract Manifest XrManifest { get; }
	}
}
