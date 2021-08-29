using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Components;
using osu.Framework.XR.Extensions;
using osu.Framework.XR.Graphics;
using osuTK;
using System.Linq;

namespace osu.Framework.XR.Testing.Components {
	public class TriangleIndicator : CompositeDrawable {
		public readonly BindableBool AllowDraggingBindable = new BindableBool( true );
		public bool AllowDragging {
			get => AllowDraggingBindable.Value;
			set => AllowDraggingBindable.Value = value;
		}

		private readonly BindableWithCurrent<Vector3> a = new BindableWithCurrent<Vector3>();
		private readonly BindableWithCurrent<Vector3> b = new BindableWithCurrent<Vector3>();
		private readonly BindableWithCurrent<Vector3> c = new BindableWithCurrent<Vector3>();

		public Bindable<Vector3> PointA {
			get => a;
			set => a.Current = value;
		}
		public Bindable<Vector3> PointB {
			get => b;
			set => b.Current = value;
		}
		public Bindable<Vector3> PointC {
			get => c;
			set => c.Current = value;
		}

		Model tris;

		public Vector3 Normal => a.Value == b.Value || a.Value == c.Value || b.Value == c.Value 
			? Vector3.UnitY 
			: Vector3.Cross( b.Value - a.Value, c.Value - a.Value ).Normalized();
		public Face Face => new Face( PointA.Value, PointB.Value, PointC.Value );

		public TriangleIndicator ( Scene scene ) {
			AddRangeInternal( new Drawable[] {
				new PointIndicator( scene ) {
					Current = PointA,
				},
				new PointIndicator( scene ) {
					Current = PointB
				},
				new PointIndicator( scene ) {
					Current = PointC
				},
			} );

			foreach ( var i in InternalChildren.OfType<PointIndicator>() )
				i.AllowDraggingBindable.BindTo( AllowDraggingBindable );

			scene.Add( tris = new Model { } );

			(PointA, PointB, PointC).BindValuesChanged( (a,b,c) => {
				var mesh = new Mesh();

				mesh.Vertices.Add( a );
				mesh.Vertices.Add( b );
				mesh.Vertices.Add( c );

				mesh.Tris.Add( new IndexedFace( 0, 1, 2 ) );

				tris.Mesh = mesh;
			}, true );

			AlwaysPresent = true;
		}

		public Colour4 Tint {
			get => tris.Tint;
			set => tris.Tint = value;
		}

		public Kind Kind {
			set {
				AllowDragging = value.IsEditable();
				Colour = value.MainColour();
				Tint = value.SecondaryColour();
			}
		}
	}
}
