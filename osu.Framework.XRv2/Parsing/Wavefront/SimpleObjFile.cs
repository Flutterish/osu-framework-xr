using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Vertices;
using System.Globalization;

namespace osu.Framework.XR.Parsing.Wavefront;

public static class SimpleObjFile {
	/// <summary>
	/// Loads an .obj files data as a simple mesh with position, uvs and normals.
	/// This mesh is not automatically uploaded
	/// </summary>
	public static Mesh Load ( string data )
		=> Load( data.SplitLines() );

	static MeshDescriptor meshDescriptor = new MeshDescriptor()
		.SetAttribute( 0, 0, MeshDescriptor.Position )
		.SetAttribute( 1, 0, MeshDescriptor.UV )
		.SetAttribute( 2, 0, MeshDescriptor.Normal );

	/// <inheritdoc cref="Load(string)"/>
	public static Mesh Load ( IEnumerable<string> lines ) {
		ElementBuffer<uint> EBO = new();
		VertexBuffer<PositionVertex> VBOPositions = new();
		VertexBuffer<UvVertex> VBOUVs = new();
		VertexBuffer<PositionVertex> VBONormals = new();
		var mesh = new Mesh( EBO, VBOPositions, VBOUVs, VBONormals ) { Descriptor = meshDescriptor };

		List<PositionVertex> Vertices = new();
		List<UvVertex> TextureCoordinates = new();
		List<PositionVertex> Normals = new();
		lines = lines.Select( x => x.Trim() ).Where( x => !(string.IsNullOrWhiteSpace( x ) || x.StartsWith('#')) );

		static float parse ( string? x )
			=> float.TryParse( x, NumberStyles.Any, CultureInfo.InvariantCulture, out var v ) ? v : 0;

		static int parseInt ( string? x )
			=> int.TryParse( x, NumberStyles.Any, CultureInfo.InvariantCulture, out var v ) ? v : 0;

		Dictionary<(int, int, int), uint> indices = new();
		uint getIndex ( (int pos, int uv, int norm) data ) {
			if ( !indices.TryGetValue( data, out var index ) ) {
				indices.Add( data, index = (uint)VBOPositions.Data.Count );
				var (pos, uv, norm) = data;
				VBOPositions.Data.Add( Vertices.At( pos ) );
				VBOUVs.Data.Add( TextureCoordinates.At( uv ) );
				VBONormals.Data.Add( Vertices.At( norm ) );
			}

			return index;
		}

		static int indexOf<T> ( string? data, List<T> list ) {
			var i = parseInt( data );
			return i switch {
				0 => 0,
				var z when z > 0 => z - 1,
				var z => list.Count + z
			};
		}

		var trimOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
		foreach ( var line in lines ) {
			var i = line.IndexOf( ' ' );
			var header = line[..i];
			var content = line[(i+1)..];

			switch ( header ) {
				case "v":
					var vertex = content.Split( ' ', trimOptions ).Select( parse ).ToArray();
					Vertices.Add( new( vertex.At( 0 ), vertex.At( 1 ), vertex.At( 2 ) ) );
					break;

				case "vt":
					vertex = content.Split( ' ', trimOptions ).Select( parse ).ToArray();
					TextureCoordinates.Add( new( vertex.At( 0 ), vertex.At( 1 ) ) );
					break;

				case "vn":
					vertex = content.Split( ' ', trimOptions ).Select( parse ).ToArray();
					Normals.Add( new( vertex.At( 0 ), vertex.At( 1 ), vertex.At( 2 ) ) );
					break;

				case "f":
					var face = content.Split( ' ', trimOptions ).Select( x => {
						var indces = x.Split( '/' );
						return (
							indexOf( indces.At( 0 ), Vertices ),
							indexOf( indces.At( 1 ), TextureCoordinates ),
							indexOf( indces.At( 2 ), Normals )
						);
					} ).ToArray();

					if ( face.Length == 3 ) {
						EBO.Indices.Add( getIndex( face[0] ) );
						EBO.Indices.Add( getIndex( face[1] ) );
						EBO.Indices.Add( getIndex( face[2] ) );
					}
					else if ( face.Length == 4 ) {
						EBO.Indices.Add( getIndex( face[0] ) );
						EBO.Indices.Add( getIndex( face[1] ) );
						EBO.Indices.Add( getIndex( face[2] ) );

						EBO.Indices.Add( getIndex( face[0] ) );
						EBO.Indices.Add( getIndex( face[2] ) );
						EBO.Indices.Add( getIndex( face[3] ) );
					}
					break;
			}
		}

		return mesh;
	}
}
