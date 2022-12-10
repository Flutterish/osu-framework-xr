using OpenVR.NET;
using OpenVR.NET.Input;
using OpenVR.NET.Manifest;
using osu.Framework.Bindables;
using osu.Framework.XR.Maths;
using osu.Framework.XR.VirtualReality.Devices;
using Valve.VR;

namespace osu.Framework.XR.VirtualReality;

public class VrInput {
	public readonly VrCompositor VR;
	public VrInput ( VrCompositor vr ) {
		VR = vr;
	}

	public IActionManifest? ActionManifest { get; private set; }
	protected Bindable<Hand> DominantHand = new( Hand.Right );
	public IBindable<Hand> DominantHandBindable => DominantHand;

	bool isManifestLoaded = false;
	public void SetActionManifest ( IActionManifest manifest ) {
		isManifestLoaded = false;
		ActionManifest = manifest;
		ApplyActionManifest( manifest, () => {
			isManifestLoaded = true;
			manifestLoaded?.Invoke( VR.VR! );
			manifestLoaded = null;
		} );
	}

	protected virtual void ApplyActionManifest ( IActionManifest manifest, System.Action callback ) {
		if ( VR.VR is VR vr ) {
			vr.SetActionManifest( manifest );
			vr.BindActionsLoaded( callback );
			DominantHand.Value = vr.DominantHand == ETrackedControllerRole.LeftHand ? Hand.Left : Hand.Right;
		}
		else {
			VR.Initialized += vr => {
				vr.VR!.SetActionManifest( manifest );
				vr.VR!.BindActionsLoaded( callback );
				DominantHand.Value = vr.VR!.DominantHand == ETrackedControllerRole.LeftHand ? Hand.Left : Hand.Right;
			};
		}
	}

	public void BindManifestLoaded ( Action<VR> action ) {
		if ( isManifestLoaded )
			action( VR.VR! );
		else
			manifestLoaded += action;
	}
	event Action<VR>? manifestLoaded;

	Dictionary<(Enum, Controller?), VrAction> cache = new();
	static Dictionary<Type, Func<Enum, VrInput, Controller?, VrAction>> factory = new() {
		[typeof(BooleanAction)] = (n, vr, s) => new BooleanAction( n, vr, s ),
		[typeof(ScalarAction)] = (n, vr, s) => new ScalarAction( n, vr, s ),
		[typeof(Vector2Action)] = (n, vr, s) => new Vector2Action( n, vr, s ),
		[typeof(Vector3Action)] = (n, vr, s) => new Vector3Action( n, vr, s ),
		[typeof(PoseAction)] = (n, vr, s) => new PoseAction( n, vr, s ),
		[typeof(HandSkeletonAction)] = (n, vr, s) => new HandSkeletonAction( n, vr, s ),
		[typeof(HapticAction)] = (n, vr, s) => new HapticAction( n, vr, s )
	};

	protected virtual Dictionary<Type, Func<Enum, VrInput, Controller?, VrAction>> Factory => factory;

	public T GetAction<T> ( Enum action, Controller? source = null ) where T : VrAction {
		if ( cache.TryGetValue( (action, source), out var cached ) )
			return (T)cached;

		VrAction value = null!;

		foreach ( var (type, f) in Factory ) {
			if ( type.IsAssignableTo(typeof(T)) ) {
				value = f( action, this, source );
				break;
			}
		}

		cache.Add( (action, source), value );
		return (T)value;
	}
}

public abstract class VrAction {
	public readonly object Name;
	public readonly Controller? Source;

	public VrAction ( object name, Controller? source ) {
		Name = name;
		Source = source;
	}
}

public abstract class VrAction<T> : VrAction where T : OpenVR.NET.Input.Action {
	protected T? Backing { get; private set; }

	protected VrAction ( object name, Controller? source = null ) : base( name, source ) { }
	public VrAction ( Enum name, VrInput input, Controller? source = null ) : base( name, source ) {
		input.BindManifestLoaded( vr => {
			Backing = vr.GetAction<T>( name, source?.Source );
			if ( Backing != null )
				Loaded();
		} );
	}

	protected abstract void Loaded ();
}

public interface IVrInputAction<T> {
	IBindable<T> ValueBindable { get; }
	T Value { get; set; }
}

public abstract class VrInputAction<T, TbackingType, Tbacking> : VrAction<Tbacking>, IVrInputAction<T> where TbackingType : struct where Tbacking : InputAction<TbackingType> {
	public readonly Bindable<T> ValueBindable = new();
	public T Value {
		get => ValueBindable.Value;
		set => ValueBindable.Value = value;
	}
	IBindable<T> IVrInputAction<T>.ValueBindable => ValueBindable;

	protected VrInputAction ( object name, Controller? source = null ) : base( name, source ) { }
	public VrInputAction ( Enum name, VrInput vr, Controller? source ) : base( name, vr, source ) { }

	protected override void Loaded () {
		Backing!.ValueUpdated += v => Value = Convert( v );
	}

	protected abstract T Convert ( TbackingType value );
}

public abstract class VrInputAction<T, Tbacking> : VrInputAction<T, T, Tbacking> where T : struct where Tbacking : InputAction<T> {
	protected VrInputAction ( object name, Controller? source = null ) : base( name, source ) { }
	protected VrInputAction ( Enum name, VrInput vr, Controller? source ) : base( name, vr, source ) { }

	protected override T Convert ( T value ) {
		return value;
	}
}

public class BooleanAction : VrInputAction<bool, OpenVR.NET.Input.BooleanAction> {
	protected BooleanAction ( object name, Controller? source = null ) : base( name, source ) { }
	public BooleanAction ( Enum name, VrInput vr, Controller? source ) : base( name, vr, source ) { }
}

public class ScalarAction : VrInputAction<float, OpenVR.NET.Input.ScalarAction> {
	protected ScalarAction ( object name, Controller? source = null ) : base( name, source ) { }
	public ScalarAction ( Enum name, VrInput vr, Controller? source ) : base( name, vr, source ) { }
}

public class Vector2Action : VrInputAction<Vector2, System.Numerics.Vector2, OpenVR.NET.Input.Vector2Action> {
	protected Vector2Action ( object name, Controller? source = null ) : base( name, source ) { }
	public Vector2Action ( Enum name, VrInput vr, Controller? source ) : base( name, vr, source ) { }

	protected override Vector2 Convert ( System.Numerics.Vector2 value ) {
		return value.ToOsuTk();
	}
}

public class Vector3Action : VrInputAction<Vector3, System.Numerics.Vector3, OpenVR.NET.Input.Vector3Action> {
	protected Vector3Action ( object name, Controller? source = null ) : base( name, source ) { }
	public Vector3Action ( Enum name, VrInput vr, Controller? source ) : base( name, vr, source ) { }

	protected override Vector3 Convert ( System.Numerics.Vector3 value ) {
		return value.ToOsuTk();
	}
}

public struct PoseInput {
	/// <inheritdoc cref="Devices.VrDevice.Position"/>
	public Vector3 Position;
	/// <inheritdoc cref="Devices.VrDevice.Rotation"/>
	public Quaternion Rotation;
	/// <inheritdoc cref="Devices.VrDevice.Velocity"/>
	public Vector3 Velocity;
	/// <inheritdoc cref="Devices.VrDevice.AngularVelocity"/>
	public Vector3 AngularVelocity;

	public static PoseInput? FromOffset ( OpenVR.NET.Input.PoseInput? source, Vector3? positionOffset, Quaternion? rotationOffset ) {
		if ( source is null )
			return null;

		var pos = positionOffset ?? Vector3.Zero;
		var rot = rotationOffset ?? Quaternion.Identity;

		return new() {
			Position = rot.Apply( source.Value.Position.ToOsuTk() ) + pos,
			Rotation = rot * source.Value.Rotation.ToOsuTk(),
			Velocity = rot.Apply( source.Value.Velocity.ToOsuTk() ),
			AngularVelocity = rot.Apply( source.Value.AngularVelocity.ToOsuTk() )
		};
	}
}
public class PoseAction : VrAction<OpenVR.NET.Input.PoseAction> {
	protected PoseAction ( object name, Controller? source = null ) : base( name, source ) { }
	VrCompositor? compositor;
	public PoseAction ( Enum name, VrInput vr, Controller? source = null ) : base( name, vr, source ) {
		compositor = vr.VR;
	}

	protected override void Loaded () { }

	/// <inheritdoc cref="OpenVR.NET.Input.PoseAction.FetchData"/>
	public virtual PoseInput? FetchData ()
		=> PoseInput.FromOffset( Backing?.FetchData(), compositor?.ActivePlayer?.PositionOffset, compositor?.ActivePlayer?.RotationOffset );

	/// <inheritdoc cref="OpenVR.NET.Input.PoseAction.FetchDataForPrediction(float)"/>
	public virtual PoseInput? FetchDataForPrediction ( float secondsFromNow )
		=> PoseInput.FromOffset( Backing?.FetchDataForPrediction( secondsFromNow ), compositor?.ActivePlayer?.PositionOffset, compositor?.ActivePlayer?.RotationOffset );

	/// <inheritdoc cref="OpenVR.NET.Input.PoseAction.FetchDataForNextFrame"/>
	public virtual PoseInput? FetchDataForNextFrame ()
		=> PoseInput.FromOffset( Backing?.FetchDataForNextFrame(), compositor?.ActivePlayer?.PositionOffset, compositor?.ActivePlayer?.RotationOffset );
}

public class HandSkeletonAction : VrAction<OpenVR.NET.Input.HandSkeletonAction> {
	protected HandSkeletonAction ( object name, Controller? source = null ) : base( name, source ) { }
	public HandSkeletonAction ( Enum name, VrInput vr, Controller? source = null ) : base( name, vr, source ) { }

	protected override void Loaded () { }

	/// <inheritdoc cref="OpenVR.NET.Input.HandSkeletonAction.TrackingLevel"/>
	public virtual EVRSkeletalTrackingLevel TrackingLevel => Backing!.TrackingLevel;

	/// <inheritdoc cref="OpenVR.NET.Input.HandSkeletonAction.BoneCount"/>
	public virtual int BoneCount => Backing!.BoneCount;

	/// <inheritdoc cref="OpenVR.NET.Input.HandSkeletonAction.GetBoneName(int)"/>
	public virtual string GetBoneName ( int index )
		=> Backing!.GetBoneName( index );

	/// <inheritdoc cref="OpenVR.NET.Input.HandSkeletonAction.ParentBoneIndex(int)"/>
	public virtual int ParentBoneIndex ( int index )
		=> Backing!.ParentBoneIndex( index );

	/// <inheritdoc cref="OpenVR.NET.Input.HandSkeletonAction.GetBoneData(int)"/>
	public virtual BoneData GetBoneData ( int index )
		=> Backing!.GetBoneData( index );

	/// <inheritdoc cref="OpenVR.NET.Input.HandSkeletonAction.FetchData(EVRSkeletalTransformSpace, EVRSkeletalMotionRange)"/>
	public virtual bool FetchData (
		EVRSkeletalTransformSpace transformSpace = EVRSkeletalTransformSpace.Model,
		EVRSkeletalMotionRange motionRange = EVRSkeletalMotionRange.WithController
	) => Backing != null && Backing.FetchData( transformSpace, motionRange );

	/// <inheritdoc cref="OpenVR.NET.Input.HandSkeletonAction.FetchReferenceData(EVRSkeletalReferencePose, EVRSkeletalTransformSpace, EVRSkeletalMotionRange)"/>
	public virtual bool FetchReferenceData (
		EVRSkeletalReferencePose referencePose,
		EVRSkeletalTransformSpace transformSpace = EVRSkeletalTransformSpace.Model,
		EVRSkeletalMotionRange motionRange = EVRSkeletalMotionRange.WithController
	) => Backing != null && Backing.FetchReferenceData( referencePose, transformSpace, motionRange );

	/// <inheritdoc cref="OpenVR.NET.Input.HandSkeletonAction.GetSummary(EVRSummaryType)"/>
	public virtual HandSkeletonSummary? GetSummary ( EVRSummaryType type )
		=> Backing?.GetSummary( type );
}

public class HapticAction : VrAction<OpenVR.NET.Input.HapticAction> {
	protected HapticAction ( object name, Controller? source = null ) : base( name, source ) { }
	public HapticAction ( Enum name, VrInput vr, Controller? source = null ) : base( name, vr, source ) { }

	protected override void Loaded () { }

	/// <inheritdoc cref="OpenVR.NET.Input.HapticAction.TriggerVibration(double, double, double, double)"/>
	public virtual bool TriggerVibration ( double duration, double frequency = 40, double amplitude = 1, double delay = 0 )
		=> Backing?.TriggerVibration( duration, frequency, amplitude, delay ) ?? false;
}