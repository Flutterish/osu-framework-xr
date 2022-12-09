using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osuTK;
using System;

namespace osu.Framework.XR.Tests.Drawables;

public partial class TestSceneSwapMesh : BasicTestScene {
	BasicMesh meshA;
	BasicMesh meshB;
	BasicModel model;
	public TestSceneSwapMesh () {
	 	Scene.Add( model = new() );
		meshA = new();
		meshB = new();

		meshA.AddCircularArc( Vector3.UnitY, Vector3.UnitZ, MathF.Tau, 0.5f, 1f );
		meshB.AddCircle( Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, 32 );

		meshA.CreateFullUnsafeUpload().Enqueue();
		meshB.CreateFullUnsafeUpload().Enqueue();

		model.Mesh = meshA;
		AddStep( "Mesh A", () => model.Mesh = meshA );
		AddStep( "Mesh B", () => model.Mesh = meshB );
	}

	protected override void Dispose ( bool isDisposing ) {
		meshA.Dispose();
		meshB.Dispose();

		base.Dispose( isDisposing );
	}
}
