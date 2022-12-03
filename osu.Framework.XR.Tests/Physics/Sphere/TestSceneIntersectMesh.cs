using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Sphere;

public partial class TestSceneIntersectMesh : BasicTestScene {
	BasicModel model;
	SphereShellIndicator sphere;
	PointIndicator hit;
	TransformIndicator transform;

	public TestSceneIntersectMesh () {
		Scene.Add( model = new BasicModel { Mesh = BasicMesh.UnitCube } );
		Add( sphere = new SphereShellIndicator( Scene ) { Kind = Kind.Control } );
		Add( hit = new PointIndicator( Scene ) { Kind = Kind.Result } );
		Add( transform = new TransformIndicator( Scene ) { Kind = Kind.Control } );

		sphere.Current.Value = new Vector3( 0, 0, 1 );

		(sphere.Current, sphere.RadiusBindable, transform.PositionBindable, transform.RotationBindable).BindValuesChanged( ( origin, radius, pos, rot ) => {
			model.Position = pos;
			model.Rotation = rot;

			SphereHit sphereHit = new();
			if ( XR.Physics.Sphere.TryHit( origin, radius, model, ref sphereHit ) ) {
				hit.Current.Value = sphereHit.Point;
				hit.Alpha = 1;
			}
			else {
				hit.Alpha = 0;
			}
		}, true );
	}
}