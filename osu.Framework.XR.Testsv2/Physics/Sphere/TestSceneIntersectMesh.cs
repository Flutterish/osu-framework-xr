using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Testing;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Sphere;

public class TestSceneIntersectMesh : BasicTestScene {
	BasicModel model;
	SphereShellIndicator sphere;
	PointIndicator hit;

	public TestSceneIntersectMesh () {
		Scene.Add( model = new BasicModel { Mesh = BasicMesh.UnitCube } );
		Add( sphere = new SphereShellIndicator( Scene ) { Kind = Kind.Control } );
		Add( hit = new PointIndicator( Scene ) { Kind = Kind.Result } );

		sphere.Current.Value = new Vector3( 0, 0, 1 );

		(sphere.Current, sphere.RadiusBindable).BindValuesChanged( ( origin, radius ) => {
			if ( XR.Physics.Sphere.TryHit( origin, radius, model.Mesh, model.Matrix, out var sphereHit ) ) {
				hit.Current.Value = sphereHit.Point;
				hit.Alpha = 1;
			}
			else {
				hit.Alpha = 0;
			}
		}, true );
	}
}