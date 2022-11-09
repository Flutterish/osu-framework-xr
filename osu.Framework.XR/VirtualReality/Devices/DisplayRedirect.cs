namespace osu.Framework.XR.VirtualReality.Devices;

/// <inheritdoc cref="OpenVR.NET.Devices.DisplayRedirect"/>
public class DisplayRedirect : VrDevice<OpenVR.NET.Devices.DisplayRedirect> {
	public DisplayRedirect ( VrCompositor vr, OpenVR.NET.Devices.DisplayRedirect source ) : base( vr, source ) { }
}
