using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing;
using System;
using osuTK;

namespace osu.Framework.XR.Tests.Physics.Raycast;

public class TestSceneStressTest : BasicTestScene {
	PhysicsSystem physics;

	RayIndicator ray;
	PointIndicator hit;

	public TestSceneStressTest () {
		physics = new();
		physics.AddSubtree( Scene.Root );

		Add( ray = new RayIndicator( Scene ) { Kind = Kind.Component } );
		Add( hit = new PointIndicator( Scene ) { Kind = Kind.Result } );
		ray.LookCurrent.Value = Vector3.UnitZ;

		foreach ( var i in new[] { 100, 1_000, 10_000, 100_000 } ) {
			AddStep( $"{i} colliders", () => setColliderAmount( i ) );
		}
	}

	void setColliderAmount ( int count ) {
		Random rng = new( 65_69_81381 );
		Scene.Clear();

		for ( int i = 0; i < count; i++ ) {
			BasicModel model = new BasicModel {
				Mesh = BasicMesh.UnitCube,
				Colour = new( rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1 ),
				Position = new( 0, 0, i ),
				Scale = new( 0.4f ),
				IsColliderEnabled = true
			};
			Scene.Add( model );
		}

		GC.Collect();
	}

	protected override void Update () {
		if ( physics.TryHitRay( ray.OriginCurrent.Value, ray.Direction, out var raycastHit, ray.IsBidirectional ) ) {
			hit.Current.Value = raycastHit.Point;
			hit.Alpha = 1;
		}
		else {
			hit.Alpha = 0;
		}
	}
}
