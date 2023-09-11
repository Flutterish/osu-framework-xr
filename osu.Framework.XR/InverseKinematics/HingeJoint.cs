using osu.Framework.XR.Maths;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

	public required Vector2 AngleLimitLeftRight {
		get => angleLimitX;
		set {
			angleLimitX = value;
			var tanx = float.Tan( value.X );
			var tany = float.Tan( value.Y );
			xTangents[1] = xTangents[2] = tanx is < 0 or > 9999 ? 9999 : tanx;
			xTangents[0] = xTangents[3] = tany is < 0 or > 9999 ? 9999 : tany;
		}
	}

	public required Vector2 AngleLimitBackFront {
		get => angleLimitY;
		set {
			angleLimitY = value;
			var tanx = float.Tan( value.X );
			var tany = float.Tan( value.Y );
			yTangents[2] = yTangents[3] = tanx is < 0 or > 9999 ? 9999 : tanx;
			yTangents[0] = yTangents[1] = tany is < 0 or > 9999 ? 9999 : tany;
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
		// sqrt(a^2b^2 / (b^2x^2 + a^2y^2))

		if ( a == 0 && b == 0 )
			return 0;

		vector.X *= b;
		vector.Y *= a;
		return a * b * float.ReciprocalSqrtEstimate( vector.LengthSquared );
	}

	bool getConstrainedPosition ( Joint parent, out Vector3 constrained, out Quaternion orientationOffset ) {
		var rot = parent.Rotation * NeutralRotation;
		var ourRelativePosition = rot.Inverted().Apply( Position - parent.Position );

		bool flipped = ourRelativePosition.Z < 0;
		if ( flipped ) { // TODO implement hinge angled > 90 deg
			ourRelativePosition.Z = -ourRelativePosition.Z;
		}

		var distance = ourRelativePosition.Z;
		var vector = ourRelativePosition.Xy;
		if ( distance == 0 || ( vector.X == 0 && vector.Y == 0) ) {
			constrained = Position;
			orientationOffset = Quaternion.Identity;
			return false;
		}

		var vectorLength = vector.LengthFast;
		var normalized = vector * float.ReciprocalEstimate( vectorLength );
		var quadrant = getQuadrant( vector );

		var ellipseRadius = distance * getEllipseRadius( normalized, xTangents[quadrant], yTangents[quadrant] );
		if ( !flipped && (vectorLength <= ellipseRadius) ) {
			constrained = Position;
			orientationOffset = ourRelativePosition.LookRotation();
			why( orientationOffset );
			return false;
		}

		ourRelativePosition.Xy = normalized * ellipseRadius;
		constrained = rot.Apply( ourRelativePosition ) + parent.Position;
		orientationOffset = ourRelativePosition.LookRotation();
		why( orientationOffset );
		return true;
	}

	void why ( Quaternion q ) {
		if ( !float.IsFinite( q.W ) || !float.IsFinite( q.X ) || !float.IsFinite( q.Y ) || !float.IsFinite( q.X ) ) {

		}
	}

	public override void ConstrainParent ( Joint parent ) {
		if ( getConstrainedPosition( parent, out var position, out var orientation ) ) {
			parent.Position += Position - position;
		}
		Rotation = parent.Rotation * NeutralRotation * orientation;
	}

	public override void ConstrainSelf ( Joint parent ) {
		getConstrainedPosition( parent, out Position, out var orientation );
		Rotation = parent.Rotation * NeutralRotation * orientation;
	}
}
