using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Raycast;

public partial class TestSceneIntersectMesh : BasicTestScene {
	BasicModel model;
	RayIndicator ray;
	PointIndicator hit;
	TransformIndicator transform;

	public TestSceneIntersectMesh () {
		Scene.Add( model = new BasicModel { Mesh = BasicMesh.UnitCornerCube } );
		Add( ray = new RayIndicator( Scene ) { Kind = Kind.Control } );
		Add( hit = new PointIndicator( Scene ) { Kind = Kind.Result } );
		Add( transform = new TransformIndicator( Scene ) { Kind = Kind.Control } );

		ray.OriginCurrent.Value = new Vector3( 2, 1, 0 );
		ray.LookCurrent.Value = -Vector3.UnitY;

		(ray.OriginCurrent, ray.LookCurrent, ray.IsBidirectionalBindable, transform.PositionBindable, transform.RotationBindable).BindValuesChanged( ( origin, _, bi, pos, rot ) => {
			model.Position = pos;
			model.Rotation = rot;

			RaycastHit raycastHit = new();
			if ( XR.Physics.Raycast.TryHit( origin, ray.Direction, model, ref raycastHit, bi ) ) {
				hit.Current.Value = raycastHit.Point;
				hit.Alpha = 1;
			}
			else {
				hit.Alpha = 0;
			}
		}, true );
	}
}