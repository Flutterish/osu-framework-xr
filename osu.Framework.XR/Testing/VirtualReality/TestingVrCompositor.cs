using OpenVR.NET;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Testing.VirtualReality;

/// <summary>
/// A compositor which does not run VR
/// </summary>
public class TestingVrCompositor : VrCompositor {
	protected override Task<VR?> InitializeVr ( IReadOnlyDependencyContainer dependencies ) {
		return Task.Run( () => {
			dependencies.TryGet<VR>( out var vr );
			return vr ?? new();
		} ) as Task<VR?>;
	}
}
