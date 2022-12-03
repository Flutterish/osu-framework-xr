using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Vertices;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Framework.XR.Tests.Models;

public partial class ModelSusieCube : Model {
	static Mesh mesh;
	static ModelSusieCube () {
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
	}

	public ModelSusieCube () {
		Mesh = mesh;
	}

	protected override void Update () {
		Rotation *= Quaternion.FromAxisAngle( new Vector3( MathF.Sin( X + Y ), MathF.Cos( (float)Time.Current / 1000 ), MathF.Sin( Z - Y ) ).Normalized(), (float)Time.Elapsed / 1000 );
	}

	[BackgroundDependencyLoader]
	private void load ( TextureStore textures ) {
		var texture = textures.Get( "susie", WrapMode.ClampToEdge, WrapMode.ClampToEdge );
		Material.SetTexture( "tex", texture );
		Material.Set( "tint", new Color4( RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1 ) );
	}
}
