using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;

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

		public SceneDrawNode ( Scene source ) : base( source ) { }

		Quad screenSpaceDrawQuad;
		Shader shader = null!;
		public override void ApplyState () {
			base.ApplyState();

			screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
			shader = Source.shader;
		}

		GlHandle VAO;
		GlHandle EBO;
		GlHandle VBO;
		public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
			if ( VAO == 0 ) {
				VAO = GL.GenVertexArray();
				GL.BindVertexArray( VAO );

				VBO = GL.GenBuffer();
				GL.BindBuffer( BufferTarget.ArrayBuffer, VBO );
				var data = new float[] {
					-0.5f, -0.5f, 0.0f,
					 0.5f, -0.5f, 0.0f,
					 0.0f,  0.5f, 0.0f
				};
				GL.BufferData( BufferTarget.ArrayBuffer, data.Length * sizeof( float ), data, BufferUsageHint.StaticDraw );
				GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, 3 * sizeof( float ), 0 );
				GL.EnableVertexAttribArray( 0 );
			}
			else GL.BindVertexArray( VAO );

			shader.Bind();
			GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );
			GL.BindVertexArray( 0 );
		}
	}
}
