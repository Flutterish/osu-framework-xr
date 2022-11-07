using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Lines;

public class Path3D : BasicModel {
	private Cached isPathValid = new();
	public readonly BindableFloat PathWidth = new( 0.05f );

	public Path3D () {
		Nodes.CollectionChanged += ( _, _ ) => isPathValid.Invalidate();
		PathWidth.ValueChanged += _ => isPathValid.Invalidate();
	}

	protected readonly BindableList<Vector3> Nodes = new();
	public void ClearNodes () {
		Nodes.Clear();
	}
	public void AddNode ( Vector3 node ) {
		Nodes.Add( node );
	}
	public void AddNodes ( IEnumerable<Vector3> nodes ) {
		Nodes.AddRange( nodes );
	}

	protected override void Update () {
		base.Update();

		if ( !isPathValid.IsValid ) {
			Mesh.Clear();
			RegenerateMesh();
			Mesh.CreateFullUpload().Enqueue();
			isPathValid.Validate();
		}
	}

	protected virtual void RegenerateMesh () {
		if ( Nodes.Count == 0 ) 			return;
		else if ( Nodes.Count == 1 ) {
			Mesh.AddCircle( Nodes[0], Nodes[0].Normalized(), Nodes[0].Normalized(), 32 );
		}
		else {
			Vector3 prev = Nodes[0];
			for ( int i = 1; i < Nodes.Count; i++ ) {
				Vector3 next = Nodes[i];

				var fwd = next - prev;
				if ( !float.IsNormal( fwd.Length ) ) 
					continue;

				var up = fwd.AnyOrthogonal();

				Mesh.AddQuad( origin: prev, direction: fwd.Normalized(), up, fwd.Length, PathWidth.Value );

				prev = next;
			}
		}
	}
}