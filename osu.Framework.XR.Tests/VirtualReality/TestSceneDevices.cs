using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;

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
