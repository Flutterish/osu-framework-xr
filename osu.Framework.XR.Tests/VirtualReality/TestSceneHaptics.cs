using OpenVR.NET.Devices;
using OpenVR.NET.Manifest;
using osu.Framework.Utils;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class TestSceneHaptics : VrScene {
	public TestSceneHaptics () {
		VrCompositor.BindDeviceDetected( device => {
			if ( device is not Controller c )
				return;

			float timer = 1000;
			var haptic = VrCompositor.Input.GetAction<HapticAction>( TestingAction.Haptic, c );

			OnUpdate += _ => {
				timer -= (float)Time.Elapsed;
				if ( timer <= 0 ) {
					timer = 1000;

					haptic.TriggerVibration( 0.2f, RNG.NextDouble( 20, 100 ), RNG.NextDouble() );
				}
			};
		} );

		VrCompositor.Input.SetActionManifest( new ActionManifest<TestingCategory, TestingAction> {
			ActionSets = new() {
				new() { Name = TestingCategory.All, Type = ActionSetType.LeftRight }
			},
			Actions = new() {
				new() { Category = TestingCategory.All, Name = TestingAction.Haptic, Type = ActionType.Vibration }
			}
		} );
	}
}
