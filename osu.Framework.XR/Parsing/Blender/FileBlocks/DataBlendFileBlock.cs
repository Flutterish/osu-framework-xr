using Newtonsoft.Json.Linq;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace osu.Framework.XR.Parsing.Blender.FileBlocks {
	public class DataBlendFileBlock : BlendFileBlock {
		public DataBlendFileBlock ( BlendFileBlockHeader header, BlendFile file, Stream stream ) : base( header ) {
			position = stream.Position;
			stream.Seek( header.Size, SeekOrigin.Current );
		}

		private long position;
		[MaybeNull, NotNull]
		public Structure Structure;
		[MaybeNull, NotNull]
		public List<JObject> Data;

		public override void PostProcess ( BlendFile file, SDNABlock sdna, Stream stream ) {
			Structure = sdna.Structs[ (int)Header.SDNAIndex ];

			Data = new List<JObject>( (int)Header.Count );
			for ( int i = 0; i < Header.Count; i++ ) {
				stream.Position = position + Structure.Size * i; // sometimes structs have more data than declared, this skips their padding
				Data.Add( Structure.Parse( file, stream ) );
			}
		}
	}

	public partial class Type {
		public virtual JToken Parse ( BlendFile file, Stream stream ) {
			var data = ArrayPool<byte>.Shared.Rent( (int)Size );

			try {
				stream.EnsureRead( data, 0, (int)Size );

				if ( Name == "char" ) {
					return new JValue( data[ 0 ] );
				}
				else if ( Name == "short" ) {
					return new JValue( file.ParseI16( data, 0 ) );
				}
				else if ( Name == "int" ) {
					return new JValue( file.ParseI32( data, 0 ) );
				}
				else if ( Name == "uint64_t" ) {
					return new JValue( file.ParseU64( data, 0 ) );
				}
				else if ( Name == "float" ) {
					return new JValue( file.ParseF32( data, 0 ) );
				}
				else if ( Name == "int64_t" ) {
					return new JValue( file.ParseI64( data, 0 ) );
				}

				return new JValue( $"No parsing for type '{Name}'" );
			}
			finally {
				ArrayPool<byte>.Shared.Return( data );
			}
		}
	}

	public partial class Field {
		public JToken Parse ( BlendFile file, Stream stream ) {
			if ( IsPointer ) {
				var data = ArrayPool<byte>.Shared.Rent( (int)Size );
				try {
					stream.EnsureRead( data, 0, (int)Size );

					JValue v = new JValue( file.ParsePointer( data, 0 ) );
					v.AddAnnotation( "Pointer" );
					return v;
				}
				finally {
					ArrayPool<byte>.Shared.Return( data );
				}
			}
			else if ( IsArray ) {
				if ( Type.Name == "char" ) {
					var data = ArrayPool<byte>.Shared.Rent( (int)Size );
					try {
						stream.EnsureRead( data, 0, (int)Size );

						return new JValue( file.ParseString( data, 0, (int)Size ) );
					}
					finally {
						ArrayPool<byte>.Shared.Return( data );
					}
				}

				var arr = new JArray();
				for ( int i = 0; i < ItemCount; i++ ) {
					if ( IsPointerArray ) {
						var data = ArrayPool<byte>.Shared.Rent( (int)file.Header.PointerSize );
						try {
							stream.EnsureRead( data, 0, (int)file.Header.PointerSize );

							var v = new JValue( file.ParsePointer( data, 0 ) );
							v.AddAnnotation( "Pointer" );
							arr.Add( v );
						}
						finally {
							ArrayPool<byte>.Shared.Return( data );
						}
					}
					else {
						arr.Add( Type.Parse( file, stream ) );
					}
				}
				return arr;
			}
			else {
				return Type.Parse( file, stream );
			}
		}
	}

	public partial class Structure {
		public override JObject Parse ( BlendFile file, Stream stream ) {
			var jo = new JObject();
			foreach ( var field in Fields ) {
				jo.Add( field.Key, field.Value.Parse( file, stream ) );
			}
			return jo;
		}
	}
}
