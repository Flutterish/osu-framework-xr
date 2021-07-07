﻿using osu.Framework.Bindables;
using osu.Framework.Caching;
using osuTK;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Components {
	public class Path3D : Model {
		private Cached isPathValid = new();

		public Path3D () {
			Nodes.CollectionChanged += ( _, _ ) => isPathValid.Invalidate();
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
				Mesh.IsReady = false;
				Mesh.Clear();
				RegenerateMesh();
				Mesh.IsReady = true;
				isPathValid.Validate();
			}
		}

		protected virtual void RegenerateMesh () {
			if ( Nodes.Count == 0 ) {
				return;
			}
			else if ( Nodes.Count == 1 ) {
				Mesh.AddCircle( Nodes[ 0 ], Nodes[ 0 ].Normalized(), Nodes[ 0 ].Normalized(), 32 );
			}
			else {
				foreach ( var (a,b) in Nodes.SkipLast( 1 ).Zip( Nodes.Skip( 1 ) ) ) {
					var fwd = b - a;
					var up = Vector3.Cross( fwd, Vector3.Cross( fwd, Vector3.UnitY ) ).Normalized();

					if ( !float.IsNormal( fwd.Length ) ) continue;
					if ( !float.IsNormal( up.X ) || !float.IsNormal( up.Y ) || !float.IsNormal( up.Z ) ) up = Vector3.UnitY;

					Mesh.AddQuad( origin: a, direction: fwd.Normalized(), up, fwd.Length, 0.05f );
				}
			}
		}
	}
}
