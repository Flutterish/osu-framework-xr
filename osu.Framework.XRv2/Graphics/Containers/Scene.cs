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
					 0.5f,  0.5f, 0.0f,  // top right
					 0.5f, -0.5f, 0.0f,  // bottom right
					-0.5f, -0.5f, 0.0f,  // bottom left
					-0.5f,  0.5f, 0.0f   // top left 
				};
				GL.BufferData( BufferTarget.ArrayBuffer, data.Length * sizeof( float ), data, BufferUsageHint.StaticDraw );
				GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, 3 * sizeof( float ), 0 );
				GL.EnableVertexAttribArray( 0 );

				EBO = GL.GenBuffer();
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, EBO );
				var eData = new uint[] {
					0, 1, 3,   // first triangle
					1, 2, 3    // second triangle
				};
				GL.BufferData( BufferTarget.ElementArrayBuffer, 6 * sizeof( uint ), eData, BufferUsageHint.StaticDraw );
			}
			else GL.BindVertexArray( VAO );

			shader.Bind();
			GL.DrawElements( BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0 );
			GL.BindVertexArray( 0 );
		}
	}
}
