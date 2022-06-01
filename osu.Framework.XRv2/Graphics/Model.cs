using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Vertices;

namespace osu.Framework.XR.Graphics;

public class Model : Drawable3D {
	AttributeArray VAO = new();
	Mesh mesh;
	public Model () {
		ElementBuffer<uint> EBO = new();
		EBO.Indices.AddRange( new uint[] {
			0,  1,  2,  2,  3,  0,
			4,  5,  6,  6,  7,  4,
			8,  9,  10, 10, 4,  8,
			11, 2,  12, 12, 13, 11,
			10, 14, 5,  5,  4,  10,
			3,  2,  11, 11, 15, 3
		} );

		VertexBuffer<TexturedVertex> VBO = new();
		VBO.Data.AddRange( new TexturedVertex[] {
			new() { Position = new( -1, -1, -1 ), UV = new( 0, 0 ) },
			new() { Position = new(  1, -1, -1 ), UV = new( 1, 0 ) },
			new() { Position = new(  1,  1, -1 ), UV = new( 1, 1 ) },
			new() { Position = new( -1,  1, -1 ), UV = new( 0, 1 ) },
			new() { Position = new( -1, -1,  1 ), UV = new( 0, 0 ) },
			new() { Position = new(  1, -1,  1 ), UV = new( 1, 0 ) },
			new() { Position = new(  1,  1,  1 ), UV = new( 1, 1 ) },
			new() { Position = new( -1,  1,  1 ), UV = new( 0, 1 ) },
			new() { Position = new( -1,  1,  1 ), UV = new( 1, 0 ) },
			new() { Position = new( -1,  1, -1 ), UV = new( 1, 1 ) },
			new() { Position = new( -1, -1, -1 ), UV = new( 0, 1 ) },
			new() { Position = new(  1,  1,  1 ), UV = new( 1, 0 ) },
			new() { Position = new(  1, -1, -1 ), UV = new( 0, 1 ) },
			new() { Position = new(  1, -1,  1 ), UV = new( 0, 0 ) },
			new() { Position = new(  1, -1, -1 ), UV = new( 1, 1 ) },
			new() { Position = new( -1,  1,  1 ), UV = new( 0, 0 ) }
		} );

		mesh = new( EBO, VBO );
		mesh.CreateFullUnsafeUpload().Enqueue();

		Z = 5;
	}

	protected override void Update () {
		//Rotation = Quaternion.FromAxisAngle( new Vector3( 1, 0, 1 ).Normalized(), (float)Time.Current / 1000 );
	}

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials, TextureStore textures ) {
		material = materials.GetNew( "unlit" );
		texture = textures.Get( "susie", Framework.Graphics.OpenGL.Textures.WrapMode.ClampToEdge, Framework.Graphics.OpenGL.Textures.WrapMode.ClampToEdge );
	}
	Material material = null!;
	Texture texture = null!;

	protected override DrawNode3D? CreateDrawNode3D ()
		=> new ModelDrawNode( this );

	class ModelDrawNode : DrawNode3D {
		new protected Model Source => (Model)base.Source;

		AttributeArray VAO = new();
		Mesh mesh;
		public ModelDrawNode ( Model source ) : base( source ) {
			mesh = source.mesh;
			VAO = source.VAO;
		}

		Texture texture = null!;
		Material material = null!;
		Matrix4 matrix;
		protected override void UpdateState () {
			material = Source.material;
			matrix = Source.Matrix;
			texture = Source.texture;
		}

		public override void Draw ( object? ctx = null ) {
			if ( VAO.Handle == 0 ) {
				VAO.Bind();

				mesh.ElementBuffer!.Bind();
				mesh.VertexBuffers[0].Link( material.Shader, new int[] { material.Shader.GetAttrib( "aPos" ), material.Shader.GetAttrib( "aUv" ) } );
			}
			else VAO.Bind();

			material.Shader.Bind();
			texture.TextureGL.Bind();
			material.Shader.SetUniform( "matrix", ref matrix );
			material.Shader.SetUniform( "gProj", ((BasicDrawContext)ctx!).ProjectionMatrix );
			material.Shader.SetUniform( "subImage", texture.GetTextureRect() );
			mesh.Draw();
		}
	}
}
