using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Testing;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Raycast;

public class TestSceneIntersectMesh : BasicTestScene {
	BasicModel model;
	RayIndicator ray;
	PointIndicator hit;

	public TestSceneIntersectMesh () {
		Scene.Add( model = new BasicModel { Mesh = BasicMesh.UnitCube } );
		Add( ray = new RayIndicator( Scene ) { Kind = Kind.Control } );
		Add( hit = new PointIndicator( Scene ) { Kind = Kind.Result } );

		ray.OriginCurrent.Value = new Vector3( 2, 1, 0 );
		ray.LookCurrent.Value = -Vector3.UnitY;

		(ray.OriginCurrent, ray.LookCurrent, ray.IsBidirectionalBindable).BindValuesChanged( ( origin, _, bi ) => {
			if ( XR.Physics.Raycast.TryHit( origin, ray.Direction, model.Mesh, model.Matrix, out var raycastHit, bi ) ) {
				hit.Current.Value = raycastHit.Point;
				hit.Alpha = 1;
			}
			else {
				hit.Alpha = 0;
			}
		}, true );
	}
}