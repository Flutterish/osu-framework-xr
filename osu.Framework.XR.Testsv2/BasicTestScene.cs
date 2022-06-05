using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Graphics.Vertices;
using osu.Framework.XR.Testing;
using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Framework.XR.Tests;

public class BasicTestScene : TestScene3D {
	static List<uint> EBO = new();
	static List<TexturedVertex> VBO = new();
	static BasicTestScene () {
		EBO.AddRange( new uint[] {
			0,  1,  2,  2,  3,  0,
			4,  5,  6,  6,  7,  4,
			8,  9,  10, 10, 4,  8,
			11, 2,  12, 12, 13, 11,
			10, 14, 5,  5,  4,  10,
			3,  2,  11, 11, 15, 3
		} );
		VBO.AddRange( new TexturedVertex[] {
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
	}

	public BasicTestScene () {
		Scene.Camera.Z = -10;

		void cube ( BasicModel model ) {
			( (ElementBuffer<uint>)model.Mesh.ElementBuffer! ).Indices.AddRange( EBO );
			( (VertexBuffer<TexturedVertex>)model.Mesh.VertexBuffers[0] ).Data.AddRange( VBO );
			model.Mesh.CreateFullUnsafeUpload().Enqueue();
			Scene.Add( model );
		}
		var xAxis = new BasicModel() {
			Colour = Color4.Red,
			Scale = new( 10, 0.03f, 0.03f ),
			Origin = new( -1, 0, 0 )
		};
		cube( xAxis );
		var yAxis = new BasicModel() {
			Colour = Color4.Green,
			Scale = new( 0.03f, 10, 0.03f ),
			Origin = new( 0, -1, 0 )
		};
		cube( yAxis );
		var zAxis = new BasicModel() {
			Colour = Color4.Blue,
			Scale = new( 0.03f, 0.03f, 10 ),
			Origin = new( 0, 0, -1 )
		};
		cube( zAxis );
	}
}
