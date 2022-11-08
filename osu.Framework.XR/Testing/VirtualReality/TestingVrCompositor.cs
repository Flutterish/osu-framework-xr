using OpenVR.NET;
using OpenVR.NET.Devices;
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

	protected override VrInput CreateInput () {
		return new VirtualVrInput( this );
	}

	public void AddDevice ( VrDevice device ) {
		OnDeviceDetected( device );
	}
}
