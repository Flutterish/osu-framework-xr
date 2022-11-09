using OpenVR.NET;
using osu.Framework.Bindables;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using Valve.VR;

namespace osu.Framework.XR.Testing.VirtualReality;

public class TestingHeadset : Headset {
	class ControlHeadset : OpenVR.NET.Devices.Headset {
		public ControlHeadset ( VR vr, int index ) : base( vr, index ) { }

		new public Vector3 Position { set => base.Position = RenderPosition = new( value.X, value.Y, value.Z ); }
		new public Quaternion Rotation { set => base.Rotation = RenderRotation = new( value.X, value.Y, value.Z, value.W ); }
	}

	ControlHeadset control => (ControlHeadset)Source;
	public TestingHeadset ( VrCompositor vr ) : base( vr, new ControlHeadset( vr.VR!, 0 ) ) {
		PositionBindable.BindValueChanged( v => control.Position = v.NewValue );
		RotationBindable.BindValueChanged( v => control.Rotation = v.NewValue );
	}

	public readonly Bindable<Vector3> PositionBindable = new();
	public readonly Bindable<Quaternion> RotationBindable = new();

	public override Vector3 Position => PositionBindable.Value;
	public override Vector3 RenderPosition => PositionBindable.Value;

	public override Quaternion Rotation => RotationBindable.Value;
	public override Quaternion RenderRotation => RotationBindable.Value;

	public override OpenVR.NET.Devices.DeviceModel? Model => null;
	public override Vector3 AngularVelocity => Vector3.Zero;
	public override Vector3 Velocity => Vector3.Zero;
	public override bool GetBool ( ETrackedDeviceProperty property ) => default;
	public override T GetEnum<T> ( ETrackedDeviceProperty property ) => default;
	public override float GetFloat ( ETrackedDeviceProperty property ) => default;
	public override int GetInt ( ETrackedDeviceProperty property ) => default;
	public override string GetString ( ETrackedDeviceProperty property ) => string.Empty;
	public override ulong GetUlong ( ETrackedDeviceProperty property ) => default;
	public override bool HasProperty ( ETrackedDeviceProperty property ) => false;
}
