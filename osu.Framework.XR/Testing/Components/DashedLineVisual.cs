using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Components;
using osu.Framework.XR.Extensions;
using osuTK;

namespace osu.Framework.XR.Testing.Components {
	public class DashedLineVisual : CompositeDrawable {
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

		DashedPath3D line;
		public DashedLineVisual ( Scene scene ) {
			scene.Add( line = new() );

			(PointA, PointB).BindValuesChanged( (a,b) => {
				line.ClearNodes();
				for ( float t = 0; t <= 1; t += 0.2f ) {
					line.AddNode( a + ( b - a ) * t );
				}
			}, true );
		}

		protected override void Update () {
			line.Tint = Colour;
			base.Update();
		}

		public Kind Kind {
			set {
				line.Tint = value.AccentColour();
			}
		}
	}
}
