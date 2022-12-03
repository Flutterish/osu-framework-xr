using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics.OpenGL4;

namespace osu.Framework.XR.Tests.Parsing;

public partial class TestSceneSimpleObjImport : BasicTestScene {
	Mesh? mesh;

	protected override void LoadComplete () {
		base.LoadComplete();
		mesh = Scene.MeshStore.GetNew( "fox" );
		mesh.CreateFullUnsafeUpload().Enqueue();

		Scene.Add( new WireframeModel { Mesh = mesh } );
	}

	protected override void Dispose ( bool isDisposing ) {
		mesh?.Dispose();
		base.Dispose( isDisposing );
	}

	partial class WireframeModel : Model {
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
