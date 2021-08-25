using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json.Linq;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Parsing.Blender.FileBlocks;
using osuTK;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace osu.Framework.XR.Parsing.Blender {
	// https://archive.blender.org/development/architecture/blender-file-format/index.html
	public class BlendFile : IModelFile {
		private BlendFile() { }

		public BlendFileHeader Header;
		public readonly List<BlendFileBlock> Blocks = new();
		[MaybeNull, NotNull]
		public SDNABlock SDNA;

		public readonly Dictionary<ulong, LinkedType> MemoryMap = new();

		public IEnumerable<LinkedType> GetAllWithCode ( string code )
			=> Blocks.Where( x => x.Header.Identifier.StartsWith( code ) ).OfType<DataBlendFileBlock>().SelectMany( x => x.LinkedData );

		public IEnumerable<LinkedType> GetAllOfType ( string type )
			=> Blocks.OfType<DataBlendFileBlock>().Where( x => x.Structure.Name == type ).SelectMany( x => x.LinkedData );

		public static BlendFile FromFile ( string path ) {
			using var stream = File.Open( path, FileMode.Open, FileAccess.Read );
			return FromStream( stream );
		}

		/// <summary>
		/// Creates a <see cref="BlendFile"/> from a <see cref="Stream"/>. It does NOT dispose the stream.
		/// </summary>
		public static BlendFile FromStream ( Stream stream ) {
			BlendFile file = new();

			file.Header = parseFileHeader( stream );
			while ( stream.Position < stream.Length ) {
				var block = file.parseBlock( stream );

				if ( block.Header.Identifier == "ENDB" ) break;
			}

			if ( file.SDNA is null )
				throw new InvalidDataException( "File did not contain an SDNA block." );

			foreach ( var i in file.Blocks ) {
				i.PostProcess( file, file.SDNA, stream );
			}

			file.Blocks.Sort( ( a, b ) => a.Header.OldPointerAddress > b.Header.OldPointerAddress ? 1 : -1 );

			return file;
		}

		private static BlendFileHeader parseFileHeader ( Stream stream ) {
			var buffer = ArrayPool<byte>.Shared.Rent( 12 );

			try {
				stream.EnsureRead( buffer, 0, 12 );

				string identifier = Encoding.ASCII.GetString( buffer.AsSpan( 0, 7 ) );
				if ( identifier != "BLENDER" )
					throw new InvalidDataException( $"Expected identifier in file header to be 'BLENDER' but it was '{identifier}'" );

				return new BlendFileHeader {
					Identifier = identifier,
					PointerSize = buffer[ 7 ] switch {
						(byte)'_' => 4,
						(byte)'-' => 8,
						_ => throw new InvalidDataException( $"Expected pointer size in file header to be either '_' (32 bit) or '-' (64 bit) but it was '{(char)buffer[ 7 ]}'" )
					},
					IsLittleEndian = buffer[ 8 ] switch {
						(byte)'v' => true,
						(byte)'V' => false,
						_ => throw new InvalidDataException( $"Expected endianess in file header to be either 'v' (little endian) or 'V' (big endian) but it was '{(char)buffer[ 8 ]}'" )
					},
					Version = $"{(char)buffer[ 9 ]}.{(char)buffer[ 10 ]}{(char)buffer[ 11 ]}"
				};
			}
			finally {
				ArrayPool<byte>.Shared.Return( buffer );
			}
		}

		private BlendFileBlock parseBlock ( Stream stream ) {
			var header = parseBlockHeader( stream );
			BlendFileBlock block;

			var endpos = stream.Position + header.Size;

			if ( header.Identifier == "DNA1" ) {
				if ( SDNA is not null )
					throw new InvalidDataException( "More than 1 SDNA block found." );

				block = SDNA = new SDNABlock( header, this, stream );
			}
			else {
				block = new DataBlendFileBlock( header, this, stream );
			}

			if ( stream.Position != endpos )
				throw new InvalidDataException( $"Block of type {header.Identifier} did not read all designated bytes." );

			Blocks.Add( block );
			return block;
		}

		private BlendFileBlockHeader parseBlockHeader ( Stream stream ) {
			stream.Align( 4 );

			var size = 16 + (int)Header.PointerSize;
			var sdnaOffset = 8 + (int)Header.PointerSize;

			var buffer = ArrayPool<byte>.Shared.Rent( size );

			try {
				stream.EnsureRead( buffer, 0, size );

				string identifier = this.ParseString( buffer, 0, 4 );

				return new BlendFileBlockHeader {
					Identifier = identifier,
					Size = this.ParseU32( buffer, 4 ),
					OldPointerAddress = this.ParsePointer( buffer, 8 ),
					SDNAIndex = this.ParseU32( buffer, sdnaOffset ),
					Count = this.ParseU32( buffer, sdnaOffset + 4 )
				};
			}
			finally {
				ArrayPool<byte>.Shared.Return( buffer );
			}
		}

		public JToken ExportAsJSON () {
			JObject jo = new JObject();
			
			JObject metadata = new JObject();
			metadata.Add( "PointerSize", Header.PointerSize );
			metadata.Add( "IsLitteEndian", Header.IsLittleEndian );
			metadata.Add( "BlenderVersion", Header.Version );
			jo.Add( "Header", metadata );

			JArray def = new JArray();
			foreach ( var i in SDNA.Structs ) {
				def.Add( i.ToString() );
			}
			jo.Add( "TypeDefinitios", def );

			JArray blocks = new JArray();
			foreach ( var i in Blocks ) {
				JObject block = new();

				JObject header = new JObject();
				header.Add( "Identifier", i.Header.Identifier );
				header.Add( "Size", i.Header.Size );
				header.Add( "Address", i.Header.OldPointerAddress );
				header.Add( "SDNAIndex", i.Header.SDNAIndex );
				header.Add( "Count", i.Header.Count );
				block.Add( "Header", header );

				if ( i is DataBlendFileBlock data ) {
					header.Add( "Structure", data.Structure.Name );

					JArray structs = new JArray();
					foreach ( var n in data.Data ) {
						structs.Add( n );
					}
					block.Add( "Elements", structs );
				}
				else {
					block.Add( "Data", "See Type Definitions" );
				}
				blocks.Add( block );
			}
			jo.Add( "Blocks", blocks );

			return jo;
		}

		public ImportedModelGroup CreateModelGroup () {
			var scene = new ImportedModelGroup( "Scene" );

			foreach ( LinkedStructure rawMesh in GetAllOfType( "Mesh" ) ) {
				LinkedStructure id = (rawMesh[ "id" ] as LinkedStructure)!;

				var totVert = rawMesh[ "totvert" ]!.GetValue<int>()!; // Vertices
				var totEdge = rawMesh[ "totedge" ]!.GetValue<int>()!; // Edges ( 2 vertices )
				var totFace = rawMesh[ "totface" ]!.GetValue<int>()!; // Tris ( and quads? )
				var totPoly = rawMesh[ "totpoly" ]!.GetValue<int>()!; // N-gons defined as spans of edge loops
				var totLoop = rawMesh[ "totloop" ]!.GetValue<int>()!; // Edge loops ( vertice + edge )
																	  // MLoopTri defines a triangle in an N-gon

				var model = new ImportedModel( ((string)id[ "name" ]!.Value!).FromCString() );
				var mat = ImportedMaterial.Default;
				var mesh = new Mesh();

				if ( totVert > 0 ) {
					var mvert = rawMesh[ "mvert" ] as LinkedArray;
					for ( int i = 0; i < totVert; i++ ) {
						var coords = ( mvert![ i ] as LinkedStructure )![ "co" ] as LinkedArray;
						mesh.Vertices.Add( new Vector3(
							coords![ 0 ]!.GetValue<float>(),
							coords![ 1 ]!.GetValue<float>(),
							coords![ 2 ]!.GetValue<float>()
						) );
					}
				}

				List<(int a, int b)> edges = new();
				if ( totEdge > 0 ) {
					var medge = rawMesh[ "medge" ] as LinkedArray;
					for ( int i = 0; i < totEdge; i++ ) {
						var edge = ( medge![ i ] as LinkedStructure )!;
						edges.Add( (
							edge[ "v1" ]!.GetValue<int>(),
							edge[ "v2" ]!.GetValue<int>()
						) );
					}
				}

				List<(int v, int e)> edgeLoops = new();
				if ( totLoop > 0 ) {
					var mloop = rawMesh[ "mloop" ] as LinkedArray;
					for ( int i = 0; i < totLoop; i++ ) {
						var loop = ( mloop![ i ] as LinkedStructure )!;
						edgeLoops.Add( (
							loop[ "v" ]!.GetValue<int>(),
							loop[ "e" ]!.GetValue<int>()
						) );
					}
				}

				List<(int start, int count)> polys = new();
				if ( totPoly > 0 ) {
					var mpoly = rawMesh[ "mpoly" ] as LinkedArray;
					for ( int i = 0; i < totPoly; i++ ) {
						var poly = ( mpoly![ i ] as LinkedStructure )!;
						polys.Add( (
							poly[ "loopstart" ]!.GetValue<int>(),
							poly[ "totloop" ]!.GetValue<int>()
						) );
					}
				}

				foreach ( var poly in polys ) {
					List<int> vertices = new();

					for ( int i = poly.start; i < poly.start + poly.count; i++ ) {
						vertices.Add( edgeLoops[ i ].v );
					}

					var v1 = vertices[ 0 ];
					var v2 = vertices[ 1 ];
					for ( int i = 2; i < vertices.Count; i++ ) {
						var v3 = vertices[ i ];

						mesh.Tris.Add( new IndexedFace( (uint)v1, (uint)v2, (uint)v3 ) );

						v2 = v3;
					}
				}

				model.Elements.Add( (mesh, mat) );
				scene.Models.Add( model );
			}

			return scene;
		}
	}

	internal static class StreamParsingExtensions {
		/// <summary>
		/// Performs <see cref="Stream.Read(byte[], int, int)"/> and throws an exception if not enough bytes were available.
		/// </summary>
		public static void EnsureRead ( this Stream stream, byte[] buffer, int offset, int count ) {
			var read = stream.Read( buffer, offset, count );
			if ( read != count )
				throw new InvalidDataException( $"Expected at least {count} more bytes, but only {read} were available." );
		}

		public static ReadOnlySpan<byte> correntEndianess ( this BlendFile file, byte[] buffer, int offset, int length ) {
			if ( file.Header.IsLittleEndian != BitConverter.IsLittleEndian ) {
				var span = buffer.AsSpan( offset, length );
				span.Reverse();
				return span;
			}
			return buffer.AsSpan( offset, length );
		}

		public static ushort ParseU16 ( this BlendFile file, byte[] buffer, int offset )
			=> BitConverter.ToUInt16( file.correntEndianess( buffer, offset, 2 ) );

		public static uint ParseU32 ( this BlendFile file, byte[] buffer, int offset )
			=> BitConverter.ToUInt32( file.correntEndianess( buffer, offset, 4 ) );

		public static ulong ParseU64 ( this BlendFile file, byte[] buffer, int offset )
			=> BitConverter.ToUInt64( file.correntEndianess( buffer, offset, 8 ) );

		public static short ParseI16 ( this BlendFile file, byte[] buffer, int offset )
			=> BitConverter.ToInt16( file.correntEndianess( buffer, offset, 2 ) );

		public static int ParseI32 ( this BlendFile file, byte[] buffer, int offset )
			=> BitConverter.ToInt32( file.correntEndianess( buffer, offset, 4 ) );

		public static long ParseI64 ( this BlendFile file, byte[] buffer, int offset )
			=> BitConverter.ToInt64( file.correntEndianess( buffer, offset, 8 ) );

		public static ulong ParsePointer ( this BlendFile file, byte[] buffer, int offset )
			=> file.Header.PointerSize == 4 ? file.ParseU32( buffer, offset ) : file.ParseU64( buffer, offset );

		public static string ParseString ( this BlendFile file, byte[] buffer, int offset, int length )
			=> Encoding.ASCII.GetString( buffer.AsSpan( offset, length ) );

		public static float ParseF32 ( this BlendFile file, byte[] buffer, int offset )
			=> BitConverter.ToSingle( file.correntEndianess( buffer, offset, 4 ) );

		public static double ParseF64 ( this BlendFile file, byte[] buffer, int offset )
			=> BitConverter.ToDouble( file.correntEndianess( buffer, offset, 8 ) );

		public static void Align ( this Stream stream, uint multiple ) {
			var offset = stream.Position;
			var nextMultiple = ( ( offset + multiple - 1 ) / multiple ) * multiple;
			if ( nextMultiple != offset ) {
				stream.Seek( nextMultiple - offset, SeekOrigin.Current );
			}
		}

		private static readonly ObjectPool<StringBuilder> builderPool = ObjectPool.Create<StringBuilder>();
		public static string ParseCString ( this BlendFile file, Stream stream ) {
			var sb = builderPool.Get();
			var single = ArrayPool<byte>.Shared.Rent( 1 );

			try {
				while ( true ) {
					stream.EnsureRead( single, 0, 1 );
					var c = (char)single[ 0 ];
					if ( c == '\0' ) return sb.ToString();
					sb.Append( c );
				}
			}
			finally {
				ArrayPool<byte>.Shared.Return( single );
				sb.Clear();
				builderPool.Return( sb );
			}
		}

		public static string FromCString ( this string cString ) {
			var index = cString.IndexOf( '\0' );
			if ( index == -1 )
				return cString;
			else
				return cString.Substring( 0, index );
		}
	}
}
