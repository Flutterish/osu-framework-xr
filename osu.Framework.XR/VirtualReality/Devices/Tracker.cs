namespace osu.Framework.XR.VirtualReality.Devices;

/// <inheritdoc cref="OpenVR.NET.Devices.Tracker"/>
public class Tracker : VrDevice<OpenVR.NET.Devices.Tracker> {
	public Tracker ( VrCompositor vr, OpenVR.NET.Devices.Tracker source ) : base( vr, source ) { }
}
