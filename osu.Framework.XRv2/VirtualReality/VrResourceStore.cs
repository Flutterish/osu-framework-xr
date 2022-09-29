using osu.Framework.Graphics.Textures;
using System.Collections.Concurrent;

namespace osu.Framework.XR.VirtualReality;

public class VrResourceStore {
	public readonly ConcurrentDictionary<int, Task<Texture?>> Textures = new();
	// TODO unload vr textues and models after use
}
