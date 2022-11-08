using OpenVR.NET.Manifest;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Testing.VirtualReality;

public class VirtualVrInput : VrInput {
	public VirtualVrInput ( VrCompositor vr ) : base( vr ) { }

	protected override void ApplyActionManifest ( IActionManifest manifest, Action callback ) {
		
	}
}
