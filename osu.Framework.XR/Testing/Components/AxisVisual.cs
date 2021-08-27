using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Testing.Components {
	public class AxisVisual : CompositeDrawable3D {
		public AxisVisual () {
			AddRangeInternal( new Drawable3D[] {
				new Model {
					Mesh = Mesh.UnitCube,
					Tint = Color4.Red,
					Scale = new Vector3( 2, 0.02f, 0.02f ),
					AutoOffsetOrigin = new Vector3( -0.5f, 0, 0 ),
					Alpha = 0.3f
				},
				new Model {
					Mesh = Mesh.UnitCube,
					Tint = Color4.Green,
					Scale = new Vector3( 0.02f, 2, 0.02f ),
					AutoOffsetOrigin = new Vector3( 0, -0.5f, 0 ),
					Alpha = 0.3f
				},
				new Model {
					Mesh = Mesh.UnitCube,
					Tint = Color4.Blue,
					Scale = new Vector3( 0.02f, 0.02f, 2 ),
					AutoOffsetOrigin = new Vector3( 0, 0, -0.5f ),
					Alpha = 0.3f
				}
			} );
		}

		public override void Show () {
			base.Show();
			foreach ( var i in InternalChildren )
				i.Show();
		}

		public override void Hide () {
			base.Hide();
			foreach ( var i in InternalChildren )
				i.Hide();
		}
	}
}
