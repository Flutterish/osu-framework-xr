using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Components;
using osu.Framework.XR.Extensions;
using osuTK;
using System.Linq;

namespace osu.Framework.XR.Testing.Components {
	public class LineIndicator : CompositeDrawable {
		public readonly BindableBool AllowDraggingBindable = new BindableBool( true );
		public bool AllowDragging {
			get => AllowDraggingBindable.Value;
			set => AllowDraggingBindable.Value = value;
		}

		private BindableWithCurrent<Vector3> a = new BindableWithCurrent<Vector3>();
		private BindableWithCurrent<Vector3> b = new BindableWithCurrent<Vector3>();

		public Bindable<Vector3> PointA {
			get => a;
			set => a.Current = value;
		}
		public Bindable<Vector3> PointB {
			get => b;
			set => b.Current = value;
		}

		public Vector3 Direction => a.Value == b.Value ? Vector3.UnitY : ( b.Value - a.Value ).Normalized();

		Path3D line;

		public LineIndicator ( Scene scene ) {
			AddRangeInternal( new Drawable[] {
				new PointIndicator( scene ) {
					Current = PointA
				},
				new PointIndicator( scene ) {
					Current = PointB
				}
			} );

			foreach ( var i in InternalChildren.OfType<PointIndicator>() )
				i.AllowDraggingBindable.BindTo( AllowDraggingBindable );

			scene.Add( line = new Path3D() );

			(PointA, PointB).BindValuesChanged( ( a, b ) => {
				line.ClearNodes();
				line.AddNode( a );
				line.AddNode( b );
			}, true );

			AlwaysPresent = true;
		}

		public Colour4 Tint {
			get => line.Tint;
			set => line.Tint = value;
		}
	}
}
