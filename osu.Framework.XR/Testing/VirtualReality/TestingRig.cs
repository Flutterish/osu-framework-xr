﻿using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Testing.VirtualReality;

public class TestingRig : CompositeDrawable {
	public readonly BindableFloat LegLength = new( 0.8f );
	public readonly BindableFloat ArmLength = new( 0.64f );
	public readonly BindableFloat TorsoLength = new( 0.6f );
	public readonly BindableFloat NeckLength = new( 0.18f );
	public readonly BindableFloat ShoulderSpan = new( 0.4f );
	public readonly BindableFloat HipSpan = new( 0.3f );

	LineIndicator leftLeg;
	LineIndicator rightLeg;
	LineIndicator hips;
	LineIndicator torso;
	LineIndicator shoulders;
	LineIndicator leftForearm;
	LineIndicator rightForearm;
	LineIndicator leftArm;
	LineIndicator rightArm;
	LineIndicator neck;

	PointIndicator leftTarget;
	PointIndicator rightTarget;

	TransformIndicator transform;

	public TestingRig ( Scene scene ) {
		AddInternal( leftLeg = new( scene ) { Kind = Kind.Result } );
		AddInternal( rightLeg = new( scene ) { Kind = Kind.Result } );
		AddInternal( hips = new( scene ) { Kind = Kind.Result } );
		AddInternal( torso = new( scene ) { Kind = Kind.Result } );
		AddInternal( shoulders = new( scene ) { Kind = Kind.Result } );
		AddInternal( leftArm = new( scene ) { Kind = Kind.Result } );
		AddInternal( rightArm = new( scene ) { Kind = Kind.Result } );
		AddInternal( leftForearm = new( scene ) { Kind = Kind.Result } );
		AddInternal( rightForearm = new( scene ) { Kind = Kind.Result } );
		AddInternal( neck = new( scene ) { Kind = Kind.Result } );

		AddInternal( leftTarget = new( scene ) { Kind = Kind.Control } );
		AddInternal( rightTarget = new( scene ) { Kind = Kind.Control } );

		AddInternal( transform = new( scene ) { Kind = Kind.Control } );

		SetRightArmTarget( Vector3.UnitX * ShoulderSpan.Value / 2 );
		SetLeftArmTarget( -Vector3.UnitX * ShoulderSpan.Value / 2 );

		bool leftLock = false;
		leftTarget.Current.BindValueChanged( v => {
			if ( leftLock )
				return;

			leftLock = true;
			SetLeftArmTarget( v.NewValue );
			leftLock = false;
		} );

		bool rightLock = false;
		rightTarget.Current.BindValueChanged( v => {
			if ( rightLock )
				return;

			rightLock = true;
			SetRightArmTarget( v.NewValue );
			rightLock = false;
		} );

		transform.PositionBindable.BindValueChanged( v => {
			SetLeftArmTarget( leftArm.PointB.Value + v.NewValue - v.OldValue );
			SetRightArmTarget( rightArm.PointB.Value + v.NewValue - v.OldValue );
		} );

		transform.RotationBindable.BindValueChanged( v => {
			SetLeftArmTarget( ( v.NewValue.DecomposeAroundAxis( Vector3.UnitY ) * v.OldValue.DecomposeAroundAxis( Vector3.UnitY ).Inverted() ).Apply(leftArm.PointB.Value - transform.PositionBindable.Value) + transform.PositionBindable.Value );
			SetRightArmTarget( ( v.NewValue.DecomposeAroundAxis( Vector3.UnitY ) * v.OldValue.DecomposeAroundAxis( Vector3.UnitY ).Inverted() ).Apply( rightArm.PointB.Value - transform.PositionBindable.Value ) + transform.PositionBindable.Value );
		} );
	}

	(Vector3 hand, Vector3 elbow) armIK ( Vector3 shoulder, Vector3 target, float totalLength ) {
		var dir = target - shoulder;
		var distance = Math.Min( dir.Length, totalLength );

		var down = transform.RotationBindable.Value.Apply( transform.RotationBindable.Value.Inverted().Apply( dir ).AnyOrthogonal() );
		var h = MathF.Sqrt( totalLength * totalLength / 4 - distance * distance / 4 );

		return (shoulder + dir.Normalized() * distance, shoulder + dir.Normalized() * distance / 2 - h * down);
	}

	Vector3 leftShoulderPosition => apply(Vector3.UnitY * ( LegLength.Value + TorsoLength.Value ) - Vector3.UnitX * ShoulderSpan.Value / 2);
	Vector3 rightShoulderPosition => apply(Vector3.UnitY * ( LegLength.Value + TorsoLength.Value ) + Vector3.UnitX * ShoulderSpan.Value / 2);

	public void SetLeftArmTarget ( Vector3 position ) {
		var (hand, elbow) = armIK( leftShoulderPosition, position, ArmLength.Value );
		leftForearm.PointB.Value = leftArm.PointA.Value = elbow;
		leftArm.PointB.Value = hand;

		leftTarget.Current.Value = hand;
	}

	public void SetRightArmTarget ( Vector3 position ) {
		var (hand, elbow) = armIK( rightShoulderPosition, position, ArmLength.Value );
		rightForearm.PointB.Value = rightArm.PointA.Value = elbow;
		rightArm.PointB.Value = hand;

		rightTarget.Current.Value = hand;
	}

	Vector3 apply ( Vector3 v ) {
		return transform.RotationBindable.Value.DecomposeAroundAxis( Vector3.UnitY ).Apply( v ) + transform.PositionBindable.Value;
	}

	protected override void Update () {
		base.Update();

		rightLeg.PointA.Value = hips.PointA.Value = apply(Vector3.UnitY * LegLength.Value + Vector3.UnitX * HipSpan.Value / 2);
		leftLeg.PointA.Value = hips.PointB.Value = apply(Vector3.UnitY * LegLength.Value - Vector3.UnitX * HipSpan.Value / 2);

		rightLeg.PointB.Value = apply(Vector3.UnitX * HipSpan.Value / 2);
		leftLeg.PointB.Value = apply(- Vector3.UnitX * HipSpan.Value / 2);

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
	}
}