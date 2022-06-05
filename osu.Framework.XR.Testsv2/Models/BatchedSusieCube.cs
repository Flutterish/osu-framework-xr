using osu.Framework.Utils;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Shaders;
using osu.Framework.XR.Graphics.Vertices;
using osu.Framework.XR.Tests.Models;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Framework.XR.Graphics;

public class BatchedSusieCube : Drawable3D {
	public static AttributeArray VAO = new();
	public static Mesh Mesh;
	static BatchedSusieCube () {
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

		Mesh = new( EBO, VBO );
		Mesh.CreateFullUnsafeUpload().Enqueue();
	}

	public BatchedSusieCube () {
		RenderStage = TestingRenderStage.SusieCubeBatch;
		color = new Color4( RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1 );
	}

	Color4 color;
	protected override void Update () {
		Rotation *= Quaternion.FromAxisAngle( new Vector3( MathF.Sin( X + Y ), MathF.Cos( (float)Time.Current / 1000 ), MathF.Sin( Z - Y ) ).Normalized(), (float)Time.Elapsed / 1000 );
	}

	protected override DrawNode3D? CreateDrawNode3D ()
		=> new BatchedSusieCubeDrawNode( this );

	class BatchedSusieCubeDrawNode : DrawNode3D {
		new protected BatchedSusieCube Source => (BatchedSusieCube)base.Source;

		public BatchedSusieCubeDrawNode ( BatchedSusieCube source ) : base( source ) {
			color = source.color;
		}

		Color4 color;
		Matrix4 matrix;
		protected override void UpdateState () {
			matrix = Source.Matrix;
		}

		public override void Draw ( object? ctx = null ) {
			var shader = (Shader)ctx!;

			shader.SetUniform( "mMatrix", ref matrix );
			shader.SetUniform( "tint", ref color );
		}
	}
}
