using osu.Framework.XR.Components;
using osu.Framework.XR.Extensions;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Testing;
using osu.Framework.XR.Testing.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Physics.Shpere {
	public class TestSceneIntersectMesh : TestScene3D {
		Model model;
		SphereShellIndicator sphere;
		PointIndicator hit;

		public TestSceneIntersectMesh () {
			Add( model = new Model { Mesh = Mesh.UnitCube } );
			Add( sphere = new SphereShellIndicator( Scene ) { Colour = Color4.Red, Tint = Color4.Orange } );
			Add( hit = new PointIndicator( Scene ) { Colour = Color4.Violet, AllowDragging = false } );

			sphere.Current.Value = new Vector3( 0, 0, 1 );

			(sphere.Current, sphere.RadiusBindable).BindValuesChanged( ( origin, radius ) => {
				if ( XR.Physics.Sphere.TryHit( origin, radius, model, out var sphereHit ) ) {
					hit.Current.Value = sphereHit.Point;
					hit.Alpha = 1;
				}
				else {
					hit.Alpha = 0;
				}
			}, true );
		}
	}
}
