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
		protected override void UpdateState () {
			material = Source.material;
		}

		public override void Draw () {
			GL.Disable( EnableCap.DepthTest );

			if ( VAO.Handle == 0 ) {
				VAO.Bind();

				mesh.CreateFullUnsafeUpload().Upload(); // this also binds the VBOs and the EBO
				mesh.VertexBuffers[0].Link( material.Shader, new int[] { material.Shader.GetAttrib( "aPos" ) } );
			}
			else VAO.Bind();

			material.Shader.Bind();
			mesh.Draw();

			GL.Enable( EnableCap.DepthTest );
		}
	}
}
