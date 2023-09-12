using osu.Framework.XR.Maths;

namespace osu.Framework.XR.InverseKinematics;

public class FabrikSolver {
	class Chain : List<Joint> {
		public readonly List<float> Lengths = new();
	}

	List<List<Chain>> SubBases = new();

	public FabrikSolver ( Joint root ) {
		HashSet<Joint> visited = new() { root };

		visitSubBase( root );
		void visitSubBase ( Joint subBase ) {
			var branches = new List<Chain>();
			SubBases.Add( branches );
			foreach ( var link in subBase.Links ) {
				var chain = new Chain { subBase };
				branches.Add( chain );
				visit( subBase, link, chain );
			}
		}
		void visit ( Joint parent, Link jointLink, Chain chain ) {
			var joint = jointLink.Target;
			if ( !visited.Add( joint ) )
				throw new InvalidDataException( "IK chain must be acyclic." );

			chain.Add( joint );
			chain.Lengths.Add( jointLink.Length );

			if ( joint.Links.Count > 1 ) {
				visitSubBase( joint );
			}
			else if ( joint.Links.Count == 1 ) {
				visit( joint, joint.Links[0], chain );
			}
		}

		if ( SubBases[0].Count == 0 )
			SubBases[0].Add( new() { root } );
	}

	public float Solve ( IEnumerable<(Joint joint, Vector3 targetPosition, Quaternion targetRotation)> targets, float tolerance = 0.001f, int maxIterations = 32 ) {
		var meanSquareError = float.MaxValue;

		while ( maxIterations != 0 ) {
			maxIterations--;

			foreach ( var (joint, target, rotation) in targets ) {
				joint.Position = target;
				joint.Rotation = rotation;
			}

			backwardPass();

			foreach ( var (joint, target, rotation) in targets ) {
				joint.Position = target;
				joint.Rotation = rotation;
			}

			forwardPass();

			var newError = targets.Sum( x => (x.joint.Position - x.targetPosition).LengthSquared );
			if ( float.Abs( newError - meanSquareError ) < tolerance ) {
				meanSquareError = newError;
				break;
			}
			meanSquareError = newError;
		}

		return meanSquareError;
	}

	void reorient ( Joint parent, Joint child, Vector3 direction ) { // makes child face direction given a base parent rotation
		if ( direction.LengthSquared < 0.1f )
			return;

		var localDirection = parent.Rotation.Inverted().Apply( direction );

		var rot = parent.Rotation * localDirection.LookRotation();
		if ( ( rot.Apply( Vector3.UnitZ ) - direction ).LengthSquared < 0.1f )
			child.Rotation = rot;
	}

	void backwardPass () {
		for ( int subBaseIndex = SubBases.Count - 1; subBaseIndex >= 0; subBaseIndex-- ) {
			var subBaseChains = SubBases[subBaseIndex];
			var subBase = subBaseChains[0][0];

			var initialSubBasePosition = subBase.Position;
			var centroidSum = Vector3.Zero;
			for ( int chainIndex = subBaseChains.Count - 1; chainIndex >= 0; chainIndex-- ) {
				var chain = subBaseChains[chainIndex];
				var lengths = chain.Lengths;
				for ( int i = chain.Count - 1; i > 0; ) {
					var fixedJoint = chain[i];
					i--;
					var nextJoint = chain[i];
					var length = lengths[i];
					fixedJoint.ConstrainParent( nextJoint );

					var direction = nextJoint.Position - fixedJoint.Position;
					direction.NormalizeFast();
					nextJoint.Position = fixedJoint.Position + direction * length;

					// make fixed joint face away from nextJoint
					//reorient( nextJoint, fixedJoint, -direction );
				}

				centroidSum += subBase.Position;
				subBase.Position = initialSubBasePosition;
			}

			var centroid = centroidSum / subBaseChains.Count;
			subBase.Position = centroid;
		}
	}

	void forwardPass () {
		for ( int subBaseIndex = 0; subBaseIndex < SubBases.Count; subBaseIndex++ ) {
			var subBaseChains = SubBases[subBaseIndex];
			
			for ( int chainIndex = 0; chainIndex < subBaseChains.Count; chainIndex++ ) {
				var chain = subBaseChains[chainIndex];
				var lengths = chain.Lengths;
				for ( int i = 0; i < chain.Count - 1; ) {
					var fixedJoint = chain[i];
					var length = lengths[i];
					i++;
					var nextJoint = chain[i];
					nextJoint.ConstrainSelf( fixedJoint );

					var direction = nextJoint.Position - fixedJoint.Position;
					direction.NormalizeFast();
					nextJoint.Position = fixedJoint.Position + direction * length;

					// make nextJoint face away from fixedJoint
					//reorient( fixedJoint, nextJoint, direction );
				}
			}
		}
	}
}
