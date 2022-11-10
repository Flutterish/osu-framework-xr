using OpenVR.NET;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;

namespace osu.Framework.XR.Testing.VirtualReality;

/// <summary>
/// A compositor which does not run VR
/// </summary>
public class TestingVrCompositor : VrCompositor {
	protected override Task<VR?> InitializeVr ( IReadOnlyDependencyContainer dependencies ) {
		return Task.Run( () => {
			dependencies.TryGet<VR>( out var vr );
			return vr ?? new();
		} ) as Task<VR?>;
	}

	new public VirtualVrInput Input => (VirtualVrInput)base.Input;

	protected override VrInput CreateInput () {
		return new VirtualVrInput( this );
	}

	public void AddDevice ( VrDevice device ) {
		OnDeviceDetected( device );
	}

	/// <summary>
	/// Binds inputs from a testing rig such as virtual controllers
	/// </summary>
	public virtual void AddRig ( TestingRig rig ) {
		var left = new TestingController( this, Valve.VR.ETrackedControllerRole.LeftHand );
		var right = new TestingController( this, Valve.VR.ETrackedControllerRole.RightHand );
		var head = new TestingHeadset( this );

		left.PositionBindable.BindTo( rig.LeftTarget.PositionBindable );
		left.RotationBindable.BindTo( rig.LeftTarget.RotationBindable );
		right.PositionBindable.BindTo( rig.RightTarget.PositionBindable );
		right.RotationBindable.BindTo( rig.RightTarget.RotationBindable );
		head.PositionBindable.BindTo( rig.Head.PositionBindable );
		head.RotationBindable.BindTo( rig.Head.RotationBindable );

		Input.LeftHandPosition.BindTo( rig.LeftTarget.PositionBindable );
		Input.LeftHandRotation.BindTo( rig.LeftTarget.RotationBindable );
		Input.RightHandPosition.BindTo( rig.RightTarget.PositionBindable );
		Input.RightHandRotation.BindTo( rig.RightTarget.RotationBindable );

		AddDevice( left );
		AddDevice( right );
		AddDevice( head );
	}
}
