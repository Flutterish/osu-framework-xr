using OpenVR.NET.Devices;
using osu.Framework.Allocation;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class VrScene : BasicTestScene {
	[Cached]
	public readonly VrCompositor VrCompositor = new();

	public VrScene () {
		Add( VrCompositor );
		Scene.Add( new VrPlayer() );

		VrCompositor.Initialized += vr => {
			vr.DeviceDetected += onVrDeviceDetected;
			foreach ( var i in vr.TrackedDevices )
				onVrDeviceDetected( i );
		};
	}

	[Cached]
	public readonly VrResourceStore VrResources = new();

	void onVrDeviceDetected ( VrDevice device ) {
		if ( device is Headset )
			return;

		Scene.Add( new BasicVrDevice( device ) );
	}
}
