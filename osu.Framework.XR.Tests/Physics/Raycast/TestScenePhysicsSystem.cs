using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing;
using osuTK;
using System;

namespace osu.Framework.XR.Tests.Physics.Raycast;

public partial class TestScenePhysicsSystem : BasicTestScene {
	PhysicsSystem physics;

	RayIndicator ray;
	PointIndicator hit;

	public TestScenePhysicsSystem () {
		physics = new();
		physics.AddSubtree( Scene.Root );
		Random rng = new( 65_69_81381 );

		Add( ray = new RayIndicator( Scene ) { Kind = Kind.Control } );
		Add( hit = new PointIndicator( Scene ) { Kind = Kind.Result } );
		for ( int i = 0; i < 10; i++ ) {
			BasicModel model = new BasicModel {
				Mesh = BasicMesh.UnitCornerCube,
				Tint = new( rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1 ),
				Position = new( rng.NextSingle( -5, 5 ), rng.NextSingle( -5, 5 ), rng.NextSingle( -5, 5 ) ),
				IsColliderEnabled = true
			};
			TransformIndicator transform = new TransformIndicator( Scene ) { Kind = Kind.Control };

			transform.PositionBindable.Value = model.Position;
			transform.PositionBindable.BindValueChanged( p => {
				model.Position = p.NewValue;
				updateCollision();
			} );
			transform.RotationBindable.Value = model.Rotation;
			transform.RotationBindable.BindValueChanged( r => {
				model.Rotation = r.NewValue;
				updateCollision();
			} );

			Scene.Add( model );
			Add( transform );
		}

		ray.OriginCurrent.Value = new Vector3( 2, 1, 0 );
		ray.LookCurrent.Value = -Vector3.UnitY;

		(ray.OriginCurrent, ray.LookCurrent, ray.IsBidirectionalBindable).BindValuesChanged( updateCollision, true );
	}

	void updateCollision () {
		if ( physics.TryHitRay( ray.OriginCurrent.Value, ray.Direction, out var raycastHit, ray.IsBidirectional ) ) {
			hit.Current.Value = raycastHit.Point;
			hit.Alpha = 1;
		}
		else {
			hit.Alpha = 0;
		}
	}
}
