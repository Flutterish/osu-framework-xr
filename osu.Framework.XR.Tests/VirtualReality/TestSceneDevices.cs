using OpenVR.NET.Devices;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class TestSceneDevices : VrScene {
	public TestSceneDevices () {
		VrCompositor.Initialized += vr => {
			vr.DeviceDetected += onVrDeviceDetected;
			foreach ( var i in vr.TrackedDevices )
				onVrDeviceDetected( i );
		};
	}

	void onVrDeviceDetected ( VrDevice device ) {
		if ( device is Headset )
			return;

		Scene.Add( new BasicVrDevice( device ) );
	}
}
