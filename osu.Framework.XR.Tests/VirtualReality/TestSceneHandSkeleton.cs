using OpenVR.NET.Devices;
using OpenVR.NET.Manifest;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class TestSceneHandSkeleton : VrScene {
	public TestSceneHandSkeleton () {
		VrCompositor.Input.SetActionManifest( new ActionManifest<TestingCategory, TestingAction> {
			ActionSets = new() {
				new() { Name = TestingCategory.All, Type = ActionSetType.Single }
			},
			Actions = new() {
				new() { Category = TestingCategory.All, Name = TestingAction.HandLeft, Type = ActionType.LeftHandSkeleton },
				new() { Category = TestingCategory.All, Name = TestingAction.HandRight, Type = ActionType.RightHandSkeleton }
			}
		} );

		VrCompositor.Initialized += vr => {
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