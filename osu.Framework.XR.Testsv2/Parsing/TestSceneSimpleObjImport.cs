using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Parsing.Wavefront;
using osuTK.Graphics.OpenGL4;
using System.IO;

namespace osu.Framework.XR.Tests.Parsing;

public class TestSceneSimpleObjImport : BasicTestScene {
	Mesh mesh;
	public TestSceneSimpleObjImport () {
		mesh = SimpleObjFile.Load( File.ReadAllText( "./Resources/fox.obj" ) );
		mesh.CreateFullUnsafeUpload().Enqueue();

		Scene.Add( new WireframeModel { Mesh = mesh } );
	}

	protected override void Dispose ( bool isDisposing ) {
		mesh.Dispose();
		base.Dispose( isDisposing );
	}

	class WireframeModel : Model {
		protected override ModelDrawNode? CreateDrawNode3D ( int index ) {
			return new WireframeDrawNode( this, index );
		}

		class WireframeDrawNode : ModelDrawNode {
			public WireframeDrawNode ( Model<Mesh> source, int index ) : base( source, index ) { }

			public override void Draw ( IRenderer renderer, object? ctx = null ) {
				GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
				base.Draw( renderer, ctx );
				GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
			}
		}
	}
}
