using osu.Framework.Allocation;
using osu.Framework.XR.Testing.VirtualReality;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class VrScene : BasicTestScene {
	[Cached(typeof(VrCompositor))]
	public readonly VrCompositor VrCompositor;
	[Cached]
	public readonly VrResourceStore VrResources = new();

	public VrScene () {
		VrCompositor = UseTestingCompositor ? new TestingVrCompositor() : new VrCompositor();
		Add( VrCompositor );
		Scene.Add( new VrPlayer() );	
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );
		VrResources.Dispose();
	}

	protected virtual bool UseTestingCompositor => false;
}
