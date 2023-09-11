using osu.Framework.XR.InverseKinematics;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Testing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Tests.InverseKinematics;

public abstract partial class IkTestScene : BasicTestScene {
	FabrikSolver solver;
	Joint root;
	Dictionary<(Joint from, Joint to), LineIndicator> links = new();
	Dictionary<Joint, DashedLineVisual> orientations = new();
	Dictionary<Joint, (DashedLineVisual line, TransformIndicator target)> targets = new();
	public IkTestScene () {
		(root, var targets) = CreateIkModel();
		solver = new( root );

		visit( root, (from, to) => {
			to.Target.Position = from.Position + to.Target.Rotation.Apply( new osuTK.Vector3( 0, 0, to.Length ) );

			var line = new LineIndicator( Scene ) { Kind = Kind.Result };
			Add( line );
			links.Add( (from, to.Target), line );

			var orientation = new DashedLineVisual( Scene ) { Kind = Kind.Result };
			Add( orientation );
			orientations.Add( to.Target, orientation );
		} );

		var orientation = new DashedLineVisual( Scene ) { Kind = Kind.Result };
		Add( orientation );
		orientations.Add( root, orientation );

		foreach ( var i in targets ) {
			var line = new DashedLineVisual( Scene ) { Kind = Kind.Control };
			var target = new TransformIndicator( Scene ) { Kind = Kind.Control };
			target.PositionBindable.Value = i.Position;
			Add( target );
			this.targets.Add( i, (line, target) );
		}
	}

	protected override void Update () {
		base.Update();

		solver.Solve( targets.Select( x => (x.Key, x.Value.target.PositionBindable.Value, x.Value.target.RotationBindable.Value) ) );
		visit( root, ( from, to ) => {
			var line = links[(from, to.Target)];
			line.PointA.Value = from.Position;
			line.PointB.Value = to.Target.Position;
		} );

		foreach ( var (i, (line, target)) in targets ) {
			line.PointA.Value = i.Position;
			line.PointB.Value = target.PositionBindable.Value;
		}

		foreach ( var (joint, orientation) in orientations ) {
			orientation.PointA.Value = joint.Position;
			orientation.PointB.Value = joint.Position + joint.Rotation.Apply( osuTK.Vector3.UnitZ * 0.1f );
		}
	}

	void visit ( Joint joint, Action<Joint, Link> action ) {
		foreach ( var link in joint.Links ) {
			action( joint, link );
			visit( link.Target, action );
		}
	}

	protected abstract (Joint root, TargetCollection targets) CreateIkModel ();

	protected class TargetCollection : List<Joint> {
		public Joint NextTarget { set => Add( value ); }
	}
}
