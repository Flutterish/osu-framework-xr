using osu.Framework.XR.Maths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.InverseKinematics;

public class HingeJoint : Joint {
	// https://www.desmos.com/calculator/qmo3q2pglf
	Vector2 angleLimitX;
	Vector2 angleLimitY;

	float[] xTangents = new float[4];
	float[] yTangents = new float[4];

	public required Vector2 AngleLimitX {
		get => angleLimitX;
		set {
			angleLimitX = value;
			xTangents[1] = xTangents[2] = float.Tan( value.X );
			xTangents[0] = xTangents[3] = float.Tan( value.Y );
		}
	}

	public required Vector2 AngleLimitY {
		get => angleLimitY;
		set {
			angleLimitY = value;
			yTangents[0] = yTangents[1] = float.Tan( value.X );
			yTangents[2] = yTangents[3] = float.Tan( value.Y );
		}
	}

	public Quaternion NeutralRotation = Quaternion.Identity;

	int getQuadrant ( Vector2 vector ) => (vector.X > 0, vector.Y > 0) switch {
		(true, true) => 0,
		(true, false) => 3,
		(false, true) => 1,
		(false, false) => 2
	};

	float getEllipseRadius ( Vector2 vector, float a, float b ) {
		if ( a == 0 && b == 0 )
			return 0;

		vector.X *= b;
		vector.Y *= a;
		return a * b * float.ReciprocalSqrtEstimate( vector.LengthSquared );
	}

	bool getConstrainedPosition ( Joint parent, Quaternion rot, out Vector3 constrained ) {
		var ourRelativePosition = rot.Inverted().Apply( Position - parent.Position );

		if ( ourRelativePosition.Z < 0 ) { // TODO implement hinge angled > 90 deg
			ourRelativePosition.Z = -ourRelativePosition.Z;
		}

		var distance = ourRelativePosition.Z;
		var vector = ourRelativePosition.Xy;
		if ( vector.X == 0 && vector.Y == 0 ) {
			constrained = Position;
			return false;
		}

		var vectorLength = vector.LengthFast;
		var normalized = vector * float.ReciprocalEstimate( vectorLength );
		var quadrant = getQuadrant( vector );

		var ellipseRadius = distance * getEllipseRadius( normalized, xTangents[quadrant], yTangents[quadrant] );
		if ( vectorLength <= ellipseRadius ) {
			constrained = Position;
			return false;
		}

		ourRelativePosition.Xy = normalized * ellipseRadius;
		constrained = rot.Apply( ourRelativePosition ) + parent.Position;
		return true;
	}

	public override void ConstrainParent ( Joint parent ) {
		if ( getConstrainedPosition( parent, parent.Rotation * NeutralRotation, out var position ) ) {
			parent.Position += Position - position;
		}
	}

	public override void ConstrainSelf ( Joint parent ) {
		Rotation = parent.Rotation * NeutralRotation;
		getConstrainedPosition( parent, parent.Rotation * NeutralRotation, out Position );
	}
}
