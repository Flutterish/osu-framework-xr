using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Vertices;

namespace osu.Framework.XR.Graphics.Containers;

[Cached]
public class Scene : CompositeDrawable {
	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials, ShaderManager shaders ) {
		shader = materials.GetShader( "unlit" );
		blitShader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
	}
	Shader shader = null!;
	IShader blitShader = null!;

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

			frameBuffer = new( new[] { osuTK.Graphics.ES30.RenderbufferInternalFormat.DepthComponent32f } );
		}

		Quad screenSpaceDrawQuad;
		Vector2 size;
		osu.Framework.Graphics.OpenGL.Buffers.FrameBuffer frameBuffer;
		Shader shader = null!;
		IShader blitShader = null!;
		public override void ApplyState () {
			base.ApplyState();

			screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
			shader = Source.shader;
			blitShader = Source.blitShader;
			size = Source.DrawSize;
		}

		AttributeArray VAO = new();
		Mesh mesh;
		public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
			frameBuffer.Size = size;
			frameBuffer.Bind();
			GLWrapper.PushViewport( new( 0, 0, (int)frameBuffer.Size.X, (int)frameBuffer.Size.Y ) );
			GLWrapper.PushScissorState( false );
			GLWrapper.Clear( new( depth: 0 ) );
			GL.Disable( EnableCap.DepthTest );

			if ( VAO.Handle == 0 ) {
				VAO.Bind();

				mesh.CreateFullUnsafeUpload().Upload(); // this also binds the VBOs and the EBO
				mesh.VertexBuffers[0].Link( shader, new int[] { shader.GetAttrib( "aPos" ) } );
			}
			else VAO.Bind();

			shader.Bind();
			mesh.Draw();
			GL.BindVertexArray( 0 );

			GL.Enable( EnableCap.DepthTest );
			GLWrapper.PopScissorState();
			GLWrapper.PopViewport();
			frameBuffer.Unbind();

			blitShader.Bind();
			frameBuffer.Texture.Bind();
			DrawQuad( frameBuffer.Texture, screenSpaceDrawQuad, DrawColourInfo.Colour );
		}
	}
}
