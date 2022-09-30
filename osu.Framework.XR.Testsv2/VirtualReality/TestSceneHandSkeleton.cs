using OpenVR.NET.Devices;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class TestSceneHandSkeleton : VrScene {
	public TestSceneHandSkeleton () {
		VrCompositor.Initialized += vr => {
			vr.SetActionManifest( TestActionManifest.Value );

			vr.DeviceDetected += onVrDeviceDetected;
			foreach ( var i in vr.TrackedDevices )
				onVrDeviceDetected( i );
		};
	}

	void onVrDeviceDetected ( VrDevice device ) {
		if ( device is not Controller c )
			return;

		Scene.Add( new BasicHandSkeleton( c, c.Role is Valve.VR.ETrackedControllerRole.LeftHand ? TestingAction.HandLeft : TestingAction.HandRight ) );
	}
}