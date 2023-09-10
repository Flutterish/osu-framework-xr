using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Testing.VirtualReality;

public partial class TestingRig : CompositeDrawable {
	public readonly BindableFloat LegLength = new( 0.8f );
	public readonly BindableFloat ArmLength = new( 0.64f );
	public readonly BindableFloat TorsoLength = new( 0.6f );
	public readonly BindableFloat NeckLength = new( 0.18f );
	public readonly BindableFloat ShoulderSpan = new( 0.4f );
	public readonly BindableFloat HipSpan = new( 0.3f );

	LineIndicator leftThigh;
	LineIndicator rightThigh;
	LineIndicator leftCalf;
	LineIndicator rightCalf;
	LineIndicator hips;
	LineIndicator torso;
	LineIndicator shoulders;
	LineIndicator leftForearm;
	LineIndicator rightForearm;
	LineIndicator leftArm;
	LineIndicator rightArm;
	LineIndicator neck;

	public readonly TransformIndicator LeftTarget;
	public readonly TransformIndicator RightTarget;

	public readonly TransformIndicator Transform;

	public readonly TransformIndicator Head;

	public TestingRig ( Scene scene ) {
		AddInternal( leftThigh = new( scene ) { Kind = Kind.Result } );
		AddInternal( rightThigh = new( scene ) { Kind = Kind.Result } );
		AddInternal( leftCalf = new( scene ) { Kind = Kind.Result } );
		AddInternal( rightCalf = new( scene ) { Kind = Kind.Result } );
		AddInternal( hips = new( scene ) { Kind = Kind.Result } );
		AddInternal( torso = new( scene ) { Kind = Kind.Result } );
		AddInternal( shoulders = new( scene ) { Kind = Kind.Result } );
		AddInternal( leftArm = new( scene ) { Kind = Kind.Result } );
		AddInternal( rightArm = new( scene ) { Kind = Kind.Result } );
		AddInternal( leftForearm = new( scene ) { Kind = Kind.Result } );
		AddInternal( rightForearm = new( scene ) { Kind = Kind.Result } );
		AddInternal( neck = new( scene ) { Kind = Kind.Result } );

		AddInternal( LeftTarget = new( scene ) { Kind = Kind.Control } );
		AddInternal( RightTarget = new( scene ) { Kind = Kind.Control } );
		AddInternal( Head = new( scene ) { Kind = Kind.Control } );

		AddInternal( Transform = new( scene ) { Kind = Kind.Control } );

		SetRightArmTarget( Vector3.UnitX * ShoulderSpan.Value / 2 );
		SetLeftArmTarget( -Vector3.UnitX * ShoulderSpan.Value / 2 );

		bool leftLock = false;
		LeftTarget.PositionBindable.BindValueChanged( v => {
			if ( leftLock )
				return;

			leftLock = true;
			SetLeftArmTarget( v.NewValue );
			leftLock = false;
		} );

		bool rightLock = false;
		RightTarget.PositionBindable.BindValueChanged( v => {
			if ( rightLock )
				return;

			rightLock = true;
			SetRightArmTarget( v.NewValue );
			rightLock = false;
		} );

		Transform.PositionBindable.BindValueChanged( v => {
			SetLeftArmTarget( leftArm.PointB.Value + v.NewValue - v.OldValue );
			SetRightArmTarget( rightArm.PointB.Value + v.NewValue - v.OldValue );
		} );

		Transform.RotationBindable.BindValueChanged( v => {
			var delta = v.NewValue.DecomposeAroundAxis( Vector3.UnitY ) * v.OldValue.DecomposeAroundAxis( Vector3.UnitY ).Inverted();

			SetLeftArmTarget( delta.Apply( leftArm.PointB.Value - Transform.PositionBindable.Value) + Transform.PositionBindable.Value );
			SetRightArmTarget( delta.Apply( rightArm.PointB.Value - Transform.PositionBindable.Value ) + Transform.PositionBindable.Value );
			LeftTarget.RotationBindable.Value = delta * LeftTarget.RotationBindable.Value;
			RightTarget.RotationBindable.Value = delta * RightTarget.RotationBindable.Value;
			Head.RotationBindable.Value = delta * Head.RotationBindable.Value;
		} );
	}

	(Vector3 result, Vector3 joint) jointIK ( Vector3 attachment, Vector3 target, float totalLength ) {
		var dir = target - attachment;
		var distance = Math.Min( dir.Length, totalLength );

		var down = Transform.RotationBindable.Value.Apply( Transform.RotationBindable.Value.Inverted().Apply( dir ).AnyOrthogonal() );
		var h = MathF.Sqrt( totalLength * totalLength / 4 - distance * distance / 4 );

		return (attachment + dir.Normalized() * distance, attachment + dir.Normalized() * distance / 2 - h * down);
	}

	Vector3 leftShoulderPosition => apply(Vector3.UnitY * ( LegLength.Value + TorsoLength.Value ) - Vector3.UnitX * ShoulderSpan.Value / 2);
	Vector3 rightShoulderPosition => apply(Vector3.UnitY * ( LegLength.Value + TorsoLength.Value ) + Vector3.UnitX * ShoulderSpan.Value / 2);
	public void SetIKTarget ( Vector3 attachment, Vector3 targetPosition, Bindable<Vector3> target, float totalLength, Bindable<Vector3> jointA, Bindable<Vector3> jointB, Bindable<Vector3> result ) {
		var (resultPosition, joint) = jointIK( attachment, targetPosition, totalLength );
		jointA.Value = jointB.Value = joint;
		result.Value = resultPosition;

		target.Value = resultPosition;
	}

	public void SetLeftArmTarget ( Vector3 position ) {
		SetIKTarget( leftShoulderPosition, position, LeftTarget.PositionBindable, ArmLength.Value, leftArm.PointA, leftForearm.PointB, leftArm.PointB );
	}
	public void SetRightArmTarget ( Vector3 position ) {
		SetIKTarget( rightShoulderPosition, position, RightTarget.PositionBindable, ArmLength.Value, rightArm.PointA, rightForearm.PointB, rightArm.PointB );
	}

	Vector3 apply ( Vector3 v ) {
		return Transform.RotationBindable.Value.DecomposeAroundAxis( Vector3.UnitY ).Apply( v ) + Transform.PositionBindable.Value;
	}

	protected override void Update () {
		base.Update();

		rightThigh.PointA.Value = hips.PointA.Value = apply(Vector3.UnitY * LegLength.Value + Vector3.UnitX * HipSpan.Value / 2);
		leftThigh.PointA.Value = hips.PointB.Value = apply(Vector3.UnitY * LegLength.Value - Vector3.UnitX * HipSpan.Value / 2);

		rightCalf.PointB.Value = apply(Vector3.UnitX * HipSpan.Value / 2);
		leftCalf.PointB.Value = apply(- Vector3.UnitX * HipSpan.Value / 2);

		rightThigh.PointB.Value = rightCalf.PointA.Value = (rightThigh.PointA.Value + rightCalf.PointB.Value) / 2;
		leftThigh.PointB.Value = leftCalf.PointA.Value = (leftThigh.PointA.Value + leftCalf.PointB.Value) / 2;

		torso.PointA.Value = apply(Vector3.UnitY * LegLength.Value);
		torso.PointB.Value = apply(Vector3.UnitY * ( LegLength.Value + TorsoLength.Value ));

		shoulders.PointA.Value = rightShoulderPosition;
		shoulders.PointB.Value = leftShoulderPosition;

		leftForearm.PointA.Value = shoulders.PointB.Value;
		rightForearm.PointA.Value = shoulders.PointA.Value;

		neck.PointA.Value = apply( Vector3.UnitY * ( LegLength.Value + TorsoLength.Value ) );
		neck.PointB.Value = apply( Vector3.UnitY * ( LegLength.Value + TorsoLength.Value ) + Vector3.UnitY * NeckLength.Value );

		SetRightArmTarget( rightArm.PointB.Value );
		SetLeftArmTarget( leftArm.PointB.Value );
		Head.PositionBindable.Value = neck.PointB.Value;
	}
}
