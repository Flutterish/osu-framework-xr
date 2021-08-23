using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Components {
	public class DashedPath3D : Path3D {
		protected override void RegenerateMesh () {
			if ( Nodes.Count == 0 ) {
				return;
			}
			else if ( Nodes.Count == 1 ) {
				Mesh.AddCircle( Nodes[ 0 ], Nodes[ 0 ].Normalized(), Nodes[ 0 ].Normalized(), 32 );
			}
			else {
				Vector3 prev = Nodes[ 0 ];
				for ( int i = 1; i < Nodes.Count; i++ ) {
					Vector3 next = Nodes[ i ];

					var fwd = next - prev;
					var up = Vector3.Cross( fwd, Vector3.Cross( fwd, Vector3.UnitY ) ).Normalized();

					if ( !float.IsNormal( fwd.Length ) ) continue;
					if ( !float.IsNormal( up.X ) || !float.IsNormal( up.Y ) || !float.IsNormal( up.Z ) ) up = Vector3.UnitY;

					var direction = fwd.Normalized();

					Mesh.AddQuad( origin: prev + fwd.Length / 4 * direction, direction: direction, up, fwd.Length / 2, PathWidth.Value );

					prev = next;
				}
			}
		}
	}
}
