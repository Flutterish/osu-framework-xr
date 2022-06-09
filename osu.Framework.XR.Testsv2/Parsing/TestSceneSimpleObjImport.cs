using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Parsing.Wavefront;
using System.IO;

namespace osu.Framework.XR.Tests.Parsing;

public class TestSceneSimpleObjImport : BasicTestScene {
	Mesh mesh;
	public TestSceneSimpleObjImport () {
		mesh = SimpleObjFile.Load( File.ReadAllText( "./Resources/fox.obj" ) );
		mesh.CreateFullUnsafeUpload().Enqueue();

		Scene.Add( new Model { Mesh = mesh } );
	}

	protected override void Dispose ( bool isDisposing ) {
		mesh.Dispose();
		base.Dispose( isDisposing );
	}
}
