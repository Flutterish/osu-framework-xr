using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

public interface IMesh {
	AABox BoundingBox { get; }
}
