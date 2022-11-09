using OpenVR.NET.Manifest;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;

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

		VrCompositor.BindDeviceDetected( device => {
			if ( device is not Controller c )
				return;

			Scene.Add( new BasicHandSkeleton( c, c.Role is Valve.VR.ETrackedControllerRole.LeftHand ? TestingAction.HandLeft : TestingAction.HandRight ) );
		} );
	}
}