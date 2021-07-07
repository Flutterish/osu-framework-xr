using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.XR.Components;
using osuTK;
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.XR {
	public abstract class XrGame : Game {
		[MaybeNull, NotNull]
		public Scene Scene { get; protected set; }
		public readonly Bindable<Vector3> PlayerOrigin = new(); // TODO just move this to a player object
		public readonly Bindable<Vector3> PlayerPosition = new();
		public abstract Manifest XrManifest { get; }
	}
}
