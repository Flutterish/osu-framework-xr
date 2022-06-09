using osu.Framework.XR.Graphics;
using osu.Framework.XR.Parsing.Wavefront;
using System.IO;

namespace osu.Framework.XR.Tests.Parsing;

public class TestSceneSimpleObjImport : BasicTestScene {
	public TestSceneSimpleObjImport () {
		var mesh = SimpleObjFile.Load( File.ReadAllText( "./Resources/fox.obj" ) );
		mesh.CreateFullUnsafeUpload().Enqueue();

		Scene.Add( new Model { Mesh = mesh } );
	}
}
