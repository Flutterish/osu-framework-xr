using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Vertices;

namespace osu.Framework.XR.Graphics;

public class Model : Drawable3D {
	AttributeArray VAO = new();
	Mesh mesh;
	public Model () {
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
		mesh.CreateFullUnsafeUpload().Enqueue();
	}

	protected override void Update () {
		Rotation = Quaternion.FromAxisAngle( new Vector3( 1, 0, 1 ).Normalized(), (float)Time.Current / 1000 );
	}

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		material = materials.GetNew( "unlit" );
	}
	Material material = null!;

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
		
		Material material = null!;
		Matrix4 matrix;
		protected override void UpdateState () {
			material = Source.material;
			matrix = Source.Matrix;
		}

		public override void Draw () {
			if ( VAO.Handle == 0 ) {
				VAO.Bind();

				mesh.ElementBuffer!.Bind();
				mesh.VertexBuffers[0].Link( material.Shader, new int[] { material.Shader.GetAttrib( "aPos" ) } );
			}
			else VAO.Bind();

			material.Shader.Bind();
			material.Shader.SetUniform( "matrix", ref matrix );
			mesh.Draw();
		}
	}
}
