using OpenVR.NET.Devices;
using osu.Framework.Bindables;
using osu.Framework.XR.Maths;
using Valve.VR;
using static OpenVR.NET.Devices.Controller;

namespace osu.Framework.XR.VirtualReality.Devices;

/// <inheritdoc cref="OpenVR.NET.Devices.Controller"/>
public class Controller : VrDevice<OpenVR.NET.Devices.Controller> {
	public Controller ( VrCompositor vr, OpenVR.NET.Devices.Controller source ) : base( vr, source ) { }

	public T GetAction<T> ( Enum action ) where T : VrAction {
		return VR.Input.GetAction<T>( action, this );
	}

	public virtual ETrackedControllerRole Role => Source.Role;

	public virtual ComponentState? GetComponentState ( ComponentModel component ) => Source.GetComponentState( component );

	Dictionary<ILegacyAction, VrAction>? legacyActions;
	public virtual IEnumerable<VrAction> LegacyActions {
		get {
			if ( legacyActions != null )
				return legacyActions.Values;

			legacyActions = new();

			foreach ( var i in Source.LegacyActions ) {
				VrAction? action = i switch {
					RawButton button => new LegacyBooleanAction( button, this ),
					RawSingle single => new LegacyScalarAction( single, this ),
					RawVector2 vec2 => new LegacyVector2Action( vec2, this ),
					RawHaptic haptic => new LegacyHapticAction( haptic, this ),
					_ => null
				};

				if ( action != null )
					legacyActions.Add( i, action );
			}

			return legacyActions.Values;
		}
	}
}

public class LegacyBooleanAction : BooleanAction {
	public BindableBool IsTouched = new();

	public LegacyBooleanAction ( RawButton name, Controller? source = null ) : base( name.Type, source ) {
		name.ValueUpdated += data => {
			IsTouched.Value = data.touched;
			Value = data.pressed;
		};
	}
}

public class LegacyScalarAction : ScalarAction {
	public LegacyScalarAction ( RawSingle name, Controller? source = null ) : base( name.Type, source ) {
		name.ValueUpdated += v => Value = v;
	}
}

public class LegacyVector2Action : Vector2Action {
	public LegacyVector2Action ( RawVector2 name, Controller? source = null ) : base( name.Type, source ) {
		name.ValueUpdated += v => Value = v.ToOsuTk();
	}
}

public class LegacyHapticAction : HapticAction {
	public LegacyHapticAction ( RawHaptic name, Controller? source = null ) : base( name.Type, source ) { }

	public override bool TriggerVibration ( double duration, double frequency = 40, double amplitude = 1, double delay = 0 ) {
		RawHaptic name = (RawHaptic)Name;

		name.TriggerVibration( 0, (ushort)Math.Min(duration * 1_000_000, ushort.MaxValue) );

		return true;
	}

	public void TriggerVibration ( int axis, ushort microSeconds ) => ((RawHaptic)Name).TriggerVibration( axis, microSeconds );
}