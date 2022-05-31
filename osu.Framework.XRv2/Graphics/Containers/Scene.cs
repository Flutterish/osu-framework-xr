using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Vertices;

namespace osu.Framework.XR.Graphics.Containers;

[Cached]
public class Scene : CompositeDrawable {
	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		shader = materials.GetShader( "unlit" );
	}
	Shader shader = null!;

	SceneDrawNode? singleDrawNode;
	protected override DrawNode CreateDrawNode ()
		=> singleDrawNode ??= new SceneDrawNode( this );

	class SceneDrawNode : CompositeDrawableDrawNode {
		new protected Scene Source => (Scene)base.Source;

		public SceneDrawNode ( Scene source ) : base( source ) {
			ElementBuffer<uint> EBO = new( PrimitiveType.Triangles );
			EBO.Indices.AddRange( new uint[] {
				0, 1, 3,
				1, 2, 3
			} );
			VertexBuffer<PositionVertex> VBO = new();
			VBO.Data.AddRange( new PositionVertex[] {
				new() { Position = new(  0.5f,  0.5f, 0.0f ) },
				new() { Position = new(  0.5f, -0.5f, 0.0f ) },
				new() { Position = new( -0.5f, -0.5f, 0.0f ) },
				new() { Position = new( -0.5f,  0.5f, 0.0f ) }
			} );

			mesh = new( EBO, VBO );
		}

		Quad screenSpaceDrawQuad;
		Shader shader = null!;
		public override void ApplyState () {
			base.ApplyState();

			screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
			shader = Source.shader;
		}

		GlHandle VAO;
		Mesh mesh;
		public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
			if ( VAO == 0 ) {
				VAO = GL.GenVertexArray();
				GL.BindVertexArray( VAO );

				mesh.CreateFullUnsafeUpload().Upload(); // this also binds the VBOs and the EBO, so its safe to link it
				mesh.VertexBuffers[0].Link( shader, new int[] { shader.GetAttrib( "aPos" ) } );
			}
			else GL.BindVertexArray( VAO );

			shader.Bind();
			mesh.Draw();
			GL.BindVertexArray( 0 );
		}
	}
}
