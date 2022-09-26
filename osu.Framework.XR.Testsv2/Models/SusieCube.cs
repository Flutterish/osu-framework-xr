using osu.Framework.Allocation;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Vertices;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Framework.XR.Graphics;

public class SusieCube : Drawable3D {
	AttributeArray VAO = new();
	Mesh mesh;
	public SusieCube () {
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
		mesh.Descriptor = BasicMesh.Descriptor;
		mesh.CreateFullUnsafeUpload().Enqueue();

		Z = 5;
	}

	protected override void Update () {
		Rotation *= Quaternion.FromAxisAngle( new Vector3( MathF.Sin( X + Y ), MathF.Cos( (float)Time.Current / 1000 ), MathF.Sin( Z - Y ) ).Normalized(), (float)Time.Elapsed / 1000 );
	}

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials, TextureStore textures ) {
		material = materials.GetNew( "unlit" );
		texture = textures.Get( "susie", WrapMode.ClampToEdge, WrapMode.ClampToEdge );

		material.CreateUpload( m => {
			m.SetUniform( "tex", texture );
			m.SetUniform( "tint", new Color4( RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1 ) );
			m.SetUniform( "subImage", texture.GetTextureRect() );
		} ).Enqueue();
	}
	Material material = null!;
	Texture texture = null!;

	protected override DrawNode3D? CreateDrawNode3D ()
		=> new SusieCubeDrawNode( this );

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			VAO.Dispose();
			mesh.Dispose();
		}
		base.Dispose( isDisposing );
	}

	class SusieCubeDrawNode : DrawNode3D {
		new protected SusieCube Source => (SusieCube)base.Source;

		AttributeArray VAO;
		Mesh mesh;
		public SusieCubeDrawNode ( SusieCube source ) : base( source ) {
			mesh = source.mesh;
			VAO = source.VAO;
		}

		Material material = null!;
		Matrix4 matrix;
		protected override void UpdateState () {
			material = Source.material;
			matrix = Source.Matrix;
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( VAO.Bind() ) {
				LinkAttributeArray( mesh, material );
			}

			material.Bind();
			material.Shader.SetUniform( "mMatrix", ref matrix );
			mesh.Draw();
		}
	}
}
