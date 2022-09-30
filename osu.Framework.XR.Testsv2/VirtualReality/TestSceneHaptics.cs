﻿using OpenVR.NET.Devices;
using OpenVR.NET.Input;
using OpenVR.NET.Manifest;
using osu.Framework.Utils;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class TestSceneHaptics : VrScene {
	public TestSceneHaptics () {
		VrCompositor.Initialized += vr => {
			vr.SetActionManifest( new ActionManifest<TestingCategory, TestingAction> {
				ActionSets = new() {
					new() { Name = TestingCategory.All, Type = ActionSetType.LeftRight }
				},
				Actions = new() {
					new() { Category = TestingCategory.All, Name = TestingAction.Haptic, Type = ActionType.Vibration }
				}
			} );

			vr.DeviceDetected += onVrDeviceDetected;
			foreach ( var i in vr.TrackedDevices )
				onVrDeviceDetected( i );
		};
	}

	void onVrDeviceDetected ( VrDevice device ) {
		if ( device is not Controller c )
			return;

		c.VR.BindActionsLoaded( () => {
			float timer = 1000;
			var haptic = c.GetAction<HapticAction>( TestingAction.Haptic )!;

			OnUpdate += _ => {
				timer -= (float)Time.Elapsed;
				if ( timer <= 0 ) {
					timer = 1000;

					haptic.TriggerVibration( 0.2f, RNG.NextDouble( 20, 100 ), RNG.NextDouble() );
				}
			};
		} );
	}
}
