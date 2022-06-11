using osu.Framework.Allocation;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class VrScene : BasicTestScene {
	[Cached]
	public readonly VrCompositor VrCompositor = new();

	public VrScene () {
		Add( VrCompositor );
		Scene.Add( new VrPlayer() );
	}
}
