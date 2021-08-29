using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Components;
using osuTK;

namespace osu.Framework.XR.Testing.Components {
	public class Simplex3ShellIndicator : CompositeDrawable {
		public readonly BindableBool AllowDraggingBindable = new BindableBool( true );
		public bool AllowDragging {
			get => AllowDraggingBindable.Value;
			set => AllowDraggingBindable.Value = value;
		}

		private readonly BindableWithCurrent<Vector3> a = new BindableWithCurrent<Vector3>();
		private readonly BindableWithCurrent<Vector3> b = new BindableWithCurrent<Vector3>();
		private readonly BindableWithCurrent<Vector3> c = new BindableWithCurrent<Vector3>();
		private readonly BindableWithCurrent<Vector3> d = new BindableWithCurrent<Vector3>();

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

		public Bindable<Vector3> PointD {
			get => d;
			set => d.Current = value;
		}

		private DashedLineVisual ab;
		private DashedLineVisual ac;
		private DashedLineVisual ad;
		private DashedLineVisual bc;
		private DashedLineVisual bd;
		private DashedLineVisual cd;

		public Simplex3ShellIndicator ( Scene scene ) {
			AddInternal( new PointIndicator( scene ) {
				Current = PointA
			} );
			AddInternal( new PointIndicator( scene ) {
				Current = PointB
			} );
			AddInternal( new PointIndicator( scene ) {
				Current = PointC
			} );
			AddInternal( new PointIndicator( scene ) {
				Current = PointD
			} );

			scene.Add( ab = new( scene ) );
			scene.Add( ac = new( scene ) );
			scene.Add( ad = new( scene ) );
			scene.Add( bc = new( scene ) );
			scene.Add( bd = new( scene ) );
			scene.Add( cd = new( scene ) );

			ab.PointA.BindTo( PointA );
			ac.PointA.BindTo( PointA );
			ad.PointA.BindTo( PointA );

			ab.PointB.BindTo( PointB );
			bc.PointA.BindTo( PointB );
			bd.PointA.BindTo( PointB );

			ac.PointB.BindTo( PointC );
			bc.PointB.BindTo( PointC );
			cd.PointA.BindTo( PointC );

			ad.PointB.BindTo( PointD );
			bd.PointB.BindTo( PointD );
			cd.PointB.BindTo( PointD );
		}

		public Colour4 Tint {
			get => ab.Colour;
			set {
				ab.Colour = value;
				ac.Colour = value;
				ad.Colour = value;
				bc.Colour = value;
				bd.Colour = value;
				cd.Colour = value;
			}
		}

		public Kind Kind {
			set {
				AllowDragging = value.IsEditable();
				Colour = value.MainColour();
				Tint = value.AccentColour();
			}
		}
	}
}
