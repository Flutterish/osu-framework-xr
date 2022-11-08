using OpenVR.NET.Devices;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class TestSceneDevices : VrScene {
	public TestSceneDevices () {
		VrCompositor.BindDeviceDetected( device => {
			if ( device is Headset )
				return;

			Scene.Add( new BasicVrDevice( device ) );
		} );
	}
}
