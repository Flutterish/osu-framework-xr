namespace osu.Framework.XR.VirtualReality.Devices;

/// <inheritdoc cref="OpenVR.NET.Devices.TrackingReference"/>
public class TrackingReference : VrDevice<OpenVR.NET.Devices.TrackingReference> {
	public TrackingReference ( VrCompositor vr, OpenVR.NET.Devices.TrackingReference source ) : base( vr, source ) { }
}
