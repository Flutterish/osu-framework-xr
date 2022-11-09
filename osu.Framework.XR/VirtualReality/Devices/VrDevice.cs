using OpenVR.NET.Devices;
using osu.Framework.Bindables;
using osu.Framework.XR.Maths;
using Valve.VR;

namespace osu.Framework.XR.VirtualReality.Devices;

/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice"/>
public class VrDevice {
	public readonly OpenVR.NET.Devices.VrDevice Source;
	public readonly VrCompositor VR;

	public VrDevice ( VrCompositor vr, OpenVR.NET.Devices.VrDevice source ) {
		Source = source;
		VR = vr;

		IsEnabled.Value = source.IsEnabled;
		source.Enabled += () => IsEnabled.Value = true;
		source.Disabled += () => IsEnabled.Value = false;

		TrackingState.Value = source.TrackingState;
		source.TrackingStateChanged += v => TrackingState.Value = v;

		Activity.Value = source.Activity;
		source.ActivityChanged += v => Activity.Value = v;
	}

	/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice.Position"/>
	public virtual Vector3 Position => Source.Position.ToOsuTk();
	/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice.Velocity"/>
	public virtual Vector3 Velocity => Source.Velocity.ToOsuTk();
	/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice.Rotation"/>
	public virtual Quaternion Rotation => Source.Rotation.ToOsuTk();
	/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice.AngularVelocity"/>
	public virtual Vector3 AngularVelocity => Source.AngularVelocity.ToOsuTk();
	/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice.RenderPosition"/>
	public virtual Vector3 RenderPosition => Source.RenderPosition.ToOsuTk();
	/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice.RenderRotation"/>
	public virtual Quaternion RenderRotation => Source.RenderRotation.ToOsuTk();

	public readonly BindableBool IsEnabled = new();
	public readonly Bindable<ETrackingResult> TrackingState = new();
	public readonly Bindable<EDeviceActivityLevel> Activity = new();

	/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice.Model"/>
	public virtual DeviceModel? Model => Source.Model;

	public virtual bool GetBool ( ETrackedDeviceProperty property ) => Source.GetBool( property );
	public virtual float GetFloat ( ETrackedDeviceProperty property ) => Source.GetFloat( property );
	public virtual int GetInt ( ETrackedDeviceProperty property ) => Source.GetInt( property );
	public virtual T GetEnum<T> ( ETrackedDeviceProperty property ) where T : struct, Enum
		=> Source.GetEnum<T>( property );
	public virtual ulong GetUlong ( ETrackedDeviceProperty property ) => Source.GetUlong( property );
	public virtual string GetString ( ETrackedDeviceProperty property ) => Source.GetString( property );

	public virtual bool HasProperty ( ETrackedDeviceProperty property ) => Source.HasProperty( property );

	public bool IsWireless => GetBool( ETrackedDeviceProperty.Prop_DeviceIsWireless_Bool );
	public bool HasBattery => GetBool( ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool );
	/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice.CanPowerOff"/>
	public bool CanPowerOff => GetBool( ETrackedDeviceProperty.Prop_DeviceCanPowerOff_Bool );
	public bool IsCharging => GetBool( ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool );
	/// <inheritdoc cref="OpenVR.NET.Devices.VrDevice.BatteryLevel"/>
	public float BatteryLevel => GetFloat( ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float );
}

/// <inheritdoc cref="VrDevice"/>
public class VrDevice<T> : VrDevice where T : OpenVR.NET.Devices.VrDevice {
	new public T Source => (T)base.Source;

	public VrDevice ( VrCompositor vr, T source ) : base( vr, source ) { }
}
