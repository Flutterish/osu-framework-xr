using osu.Framework.XR.Components;
using osu.Framework.XR.Extensions;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Testing;
using osu.Framework.XR.Testing.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Physics.Raycast {
	public class TestSceneIntersectMesh : TestScene3D {
		Model model;
		RayIndicator ray;
		PointIndicator hit;

		public TestSceneIntersectMesh () {
			Add( model = new Model { Mesh = Mesh.UnitCube } );
			Add( ray = new RayIndicator( Scene ) { Colour = Color4.Red, Tint = Color4.Orange } );
			Add( hit = new PointIndicator( Scene ) { Colour = Color4.Violet, AllowDragging = false } );

			ray.OriginCurrent.Value = new Vector3( 2, 1, 0 );
			ray.LookCurrent.Value = -Vector3.UnitY;

			(ray.OriginCurrent, ray.LookCurrent, ray.IsBidirectionalBindable).BindValuesChanged( ( origin, _, bi ) => {
				if ( XR.Physics.Raycast.TryHit( origin, ray.Direction, model, out var raycastHit, bi ) ) {
					hit.Current.Value = raycastHit.Point;
					hit.Alpha = 1;
				}
				else {
					hit.Alpha = 0;
				}
			}, true );
		}
	}
}
