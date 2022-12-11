using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Testing;
using System;
using osu.Framework.XR.Physics;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Sphere;

public partial class TestScenePhysicsSystem : BasicTestScene {
	PhysicsSystem physics;

	SphereShellIndicator sphere;
	PointIndicator hit;

	public TestScenePhysicsSystem () {
		physics = new();
		physics.AddSubtree( Scene.Root );
		Random rng = new( 678128905 );

		Add( sphere = new SphereShellIndicator( Scene ) { Kind = Kind.Control } );
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

		sphere.Current.Value = new Vector3( 0, 0, 1 );
		(sphere.Current, sphere.RadiusBindable).BindValuesChanged( updateCollision, true );
	}

	void updateCollision () {
		if ( physics.TryHitSphere( sphere.Current.Value, sphere.Radius, out var sphereHit ) ) {
			hit.Current.Value = sphereHit.Point;
			hit.Alpha = 1;
		}
		else {
			hit.Alpha = 0;
		}
	}
}
