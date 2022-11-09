namespace osu.Framework.XR.VirtualReality.Devices;

/// <inheritdoc cref="OpenVR.NET.Devices.Headset"/>
public class Headset : VrDevice<OpenVR.NET.Devices.Headset> {
	public Headset ( VrCompositor vr, OpenVR.NET.Devices.Headset source ) : base( vr, source ) { }
}
