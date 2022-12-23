using osu.Framework.XR.Collections;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Vertices;
using osu.Framework.XR.Maths;
using System.Globalization;

namespace osu.Framework.XR.Parsing.Wavefront;

public static class ObjFile {
	public class ObjMesh : Mesh, ITriangleMesh {
		public readonly VertexBuffer<PositionVertex> Positions;
		public readonly VertexBuffer<UvVertex> UVs;
		public readonly VertexBuffer<PositionVertex> Normals;
		public readonly ElementBuffer<uint> ElementBuffer;
		static MeshDescriptor meshDescriptor = new MeshDescriptor()
			.SetAttribute( 0, 0, MeshDescriptor.Position )
			.SetAttribute( 1, 0, MeshDescriptor.UV )
			.SetAttribute( 2, 0, MeshDescriptor.Normal );
		public ObjMesh ( ElementBuffer<uint> elementBuffer, VertexBuffer<PositionVertex> positions, VertexBuffer<UvVertex> uvs, VertexBuffer<PositionVertex> normals ) 
			: base( elementBuffer, positions, uvs, normals ) {
			Descriptor = meshDescriptor;
			ElementBuffer = elementBuffer;
			Positions = positions;
			UVs = uvs;
			Normals = normals;
		}

		public void RecalculateProperties () {
			var vertices = ElementBuffer.Indices.Select( x => Positions.Data[(int)x] ).Select( x => (Vector3)x );
			vertexCount = (uint)ElementBuffer.Indices.Count;
			BoundingBox = new( vertices );
		}
		public AABox BoundingBox { get; private set; }
		uint vertexCount;
		uint IGeometryMesh.VertexCount => vertexCount;

		public Vector3 GetVertexPosition ( uint index )
			=> Positions.Data[(int)ElementBuffer.Indices[(int)index]];

		public int TriangleCount => ElementBuffer.Count / 3;

		public (uint indexA, uint indexB, uint indexC) GetTriangleIndices ( int index )
			=> (ElementBuffer.Indices[index * 3], ElementBuffer.Indices[index * 3 + 1], ElementBuffer.Indices[index * 3 + 2]);
	}

	public static ImportedMeshCollection Load ( string data ) 
		=> Load( data.SplitLines() );

	public static ImportedMeshCollection Load ( IEnumerable<string> lines ) {
		ImportedMeshCollection scene = new();
		Span<Range> dataBuffer = stackalloc Range[4];
		Span<Range> innerDataBuffer = stackalloc Range[3];

		List<PositionVertex> Vertices = new();
		List<UvVertex> TextureCoordinates = new();
		List<PositionVertex> Normals = new();

		ElementBuffer<uint> EBO = new();
		VertexBuffer<PositionVertex> VBOPositions = new();
		VertexBuffer<UvVertex> VBOUVs = new();
		VertexBuffer<PositionVertex> VBONormals = new();
		scene.AllVertexBuffers.Add( VBOPositions );
		scene.AllVertexBuffers.Add( VBOUVs );
		scene.AllVertexBuffers.Add( VBONormals );

		bool isImplicitObject = true;
		ImportedObject @object = new();
		initializeObject();

		void initializeObject () {
			scene.Children.Add( @object );
			scene.AllObjects.Add( @object );
			scene.AllElementBuffers.Add( EBO );
			var mesh = new ImportedMeshPart {
				Mesh = new( new ObjMesh( EBO, VBOPositions, VBOUVs, VBONormals ) )
			};
			@object.MeshParts.Add( mesh );
			scene.AllMeshes.Add( mesh.Mesh );
		}

		void addObject ( string name ) {
			if ( isImplicitObject && EBO.Indices.Count == 0 ) {
				isImplicitObject = false;
				@object.Name = name;
				return;
			}

			isImplicitObject = false;
			@object = new() { Name = name };
			EBO = new();
			initializeObject();
		}

		Dictionary<(int, int, int), uint> indices = new();
		static int indexOf<T> ( int i, List<T> list ) {
			return i switch {
				0 => 0,
				var z when z > 0 => z - 1,
				var z => list.Count + z
			};
		}

		void addVertex ( int pos, int uv, int norm ) {
			var data = (pos, uv, norm) = (indexOf(pos, Vertices), indexOf(uv, TextureCoordinates), indexOf(norm, Normals));
			if ( !indices.TryGetValue( data, out var index ) ) {
				indices.Add( data, index = (uint)VBOPositions.Data.Count );
				VBOPositions.Data.Add( Vertices.At( pos ) );
				VBOUVs.Data.Add( TextureCoordinates.At( uv ) );
				VBONormals.Data.Add( Vertices.At( norm ) );
			}

			EBO.Indices.Add( index );
		}

		static float @float ( ReadOnlySpan<char> data )
			=> float.TryParse( data, NumberStyles.Number, CultureInfo.InvariantCulture, out var value ) ? value : 0;
		static int @int ( ReadOnlySpan<char> data )
			=> int.TryParse( data, NumberStyles.Number, CultureInfo.InvariantCulture, out var value ) ? value : 0;

		foreach ( var line in lines ) {
			var lineSpan = line.AsSpan();
			var splitIndex = lineSpan.IndexOf( ' ' );
			var header = lineSpan[..splitIndex];
			if ( header.SequenceEqual( "#" ) )
				continue;

			var data = lineSpan[(splitIndex + 1)..].Split( ' ', dataBuffer );
			if ( header.SequenceEqual( "v" ) ) {
				Vertices.Add( new( @float(data.Get(0)), @float(data.Get(1)), @float(data.Get(2)) ) );
			}
			else if ( header.SequenceEqual( "vt" ) ) {
				TextureCoordinates.Add( new( @float(data.Get(0)), @float(data.Get(1)) ) );
			}
			else if ( header.SequenceEqual( "vn" ) ) {
				Normals.Add( new( @float(data.Get(0)), @float(data.Get(1)), @float(data.Get(2)) ) );
			}
			else if ( header.SequenceEqual( "f" ) ) {
				void add ( SpanSplit data, int index, Span<Range> innerDataBuffer ) {
					var innerData = data[index].Split( '/', innerDataBuffer );
					addVertex( @int( innerData.Get( 0 ) ), @int( innerData.Get( 1 ) ), @int( innerData.Get( 2 ) ) );
				}
				if ( data.Length == 3 ) {
					add( data, 0, innerDataBuffer );
					add( data, 1, innerDataBuffer );
					add( data, 2, innerDataBuffer );
				}
				else if ( data.Length == 4 ) {
					add( data, 0, innerDataBuffer );
					add( data, 1, innerDataBuffer );
					add( data, 2, innerDataBuffer );

					add( data, 0, innerDataBuffer );
					add( data, 2, innerDataBuffer );
					add( data, 3, innerDataBuffer );
				}
			}
			else if ( header.SequenceEqual( "o" ) ) {
				addObject( new string( data[0] ) );
			}
		}

		foreach ( var i in scene.AllMeshes ) {
			((ObjMesh)i.Mesh).RecalculateProperties();
		}
		return scene;
	}
}
