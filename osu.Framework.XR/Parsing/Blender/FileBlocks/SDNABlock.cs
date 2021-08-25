using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Framework.XR.Parsing.Blender.FileBlocks {
	public class SDNABlock : BlendFileBlock {
		public readonly Dictionary<string, Type> Types = new();
		public readonly List<Structure> Structs = new();

		private struct StructDNA {
			public ushort Type;
			public (ushort Type, ushort Name)[] Fields;
		}

		public SDNABlock ( BlendFileBlockHeader header, BlendFile file, Stream stream ) : base( header ) {
			var buffer = ArrayPool<byte>.Shared.Rent( 4 );

			try {
				stream.EnsureRead( buffer, 0, 4 );
				var identifier = file.ParseString( buffer, 0, 4 );
				if ( identifier != "SDNA" )
					throw new InvalidDataException( $"Expected 1st in DNA1 block to be 'SDNA' but it was '{identifier}'" );

				stream.EnsureRead( buffer, 0, 4 );
				identifier = file.ParseString( buffer, 0, 4 );
				if ( identifier != "NAME" )
					throw new InvalidDataException( $"Expected 2nd identifier in DNA1 block to be 'NAME' but it was '{identifier}'" );

				stream.EnsureRead( buffer, 0, 4 );
				var count = file.ParseU32( buffer, 0 );
				var Names = new string[ count ];
				for ( int i = 0; i < count; i++ ) {
					Names[ i ] = file.ParseCString( stream );
				}

				stream.Align( 4 );
				stream.EnsureRead( buffer, 0, 4 );
				identifier = file.ParseString( buffer, 0, 4 );
				if ( identifier != "TYPE" )
					throw new InvalidDataException( $"Expected 3rd identifier in DNA1 block to be 'TYPE' but it was '{identifier}'" );

				stream.EnsureRead( buffer, 0, 4 );
				count = file.ParseU32( buffer, 0 );
				var Types = new string[ count ];
				for ( int i = 0; i < count; i++ ) {
					Types[ i ] = file.ParseCString( stream );
				}

				stream.Align( 4 );
				stream.EnsureRead( buffer, 0, 4 );
				identifier = file.ParseString( buffer, 0, 4 );
				if ( identifier != "TLEN" )
					throw new InvalidDataException( $"Expected 4th identifier in DNA1 block to be 'TLEN' but it was '{identifier}'" );

				var Lengths = new ushort[ count ];
				for ( int i = 0; i < count; i++ ) {
					stream.EnsureRead( buffer, 0, 2 );
					Lengths[ i ] = file.ParseU16( buffer, 0 );
				}

				stream.Align( 4 );
				stream.EnsureRead( buffer, 0, 4 );
				identifier = file.ParseString( buffer, 0, 4 );
				if ( identifier != "STRC" )
					throw new InvalidDataException( $"Expected 5th identifier in DNA1 block to be 'STRC' but it was '{identifier}'" );

				stream.EnsureRead( buffer, 0, 4 );
				count = file.ParseU32( buffer, 0 );
				var Structures = new StructDNA[count];
				for ( int i = 0; i < count; i++ ) {
					StructDNA s = new();
					stream.EnsureRead( buffer, 0, 4 );
					s.Type = file.ParseU16( buffer, 0 );
					var fieldCount = file.ParseU16( buffer, 2 );
					s.Fields = new (ushort Type, ushort Name)[fieldCount];
					for ( int k = 0; k < fieldCount; k++ ) {
						stream.EnsureRead( buffer, 0, 4 );
						s.Fields[ k ] = ( file.ParseU16( buffer, 0 ), file.ParseU16( buffer, 2 ) );
					}
					Structures[ i ] = s;
				}

				for ( int i = 0; i < Structures.Length; i++ ) {
					var s = Structures[ i ];
					var st = new Structure( Types[ s.Type ], Lengths[ s.Type ] );
					this.Structs.Add( st );
					this.Types.Add( Types[ s.Type ], st );
				}

				for ( int i = 0; i < Types.Length; i++ ) {
					if ( this.Types.ContainsKey( Types[ i ] ) ) continue;
					this.Types.Add( Types[ i ], new Type( Types[ i ], Lengths[ i ] ) );
				}

				for ( int i = 0; i < Structures.Length; i++ ) {
					var s = Structures[ i ];
					var st = (Structure)this.Types[ Types[ s.Type ] ];
					foreach ( var f in s.Fields ) {
						var field = new Field( file, Names[ f.Name ], this.Types[ Types[ f.Type ] ] );
						st.Fields.Add( field.Name, field );
					}
				}
			}
			finally {
				ArrayPool<byte>.Shared.Return( buffer );
			}
		}

		public override void PostProcess ( BlendFile file, SDNABlock sdna, Stream stream ) { }
	}

	public partial class Type {
		public Type ( string name, uint size ) {
			Name = name;
			Size = size;
		}

		public readonly string Name;
		public readonly uint Size;

		public override string ToString ()
			=> Name;
	}

	public partial class Field {
		public Field ( BlendFile file, string name, Type type ) {
			IsPointer = name.StartsWith( '*' ) || name.StartsWith( "(" );
			IsArray = name.EndsWith( ']' );
			if ( IsPointer && IsArray ) {
				IsPointer = false;
				IsPointerArray = true;
			}

			int startIndex = 0;
			int endIndex = IsArray ? name.IndexOf( '[' ) : name.Length;

			if ( name.StartsWith( "*" ) ) {
				PointerCount = name.TakeWhile( x => x == '*' ).Count();
				startIndex = PointerCount;
				if ( !IsPointer ) {
					ItemPointerCount = PointerCount;
					PointerCount = 0;
				}
			}
			else if ( name.StartsWith( "(" ) ) {
				IsFunction = true;
				startIndex = 2;
				endIndex = name.IndexOf( ")" );
				FuncArgs = name.Substring( name.LastIndexOf( '(' ) + 1, name.LastIndexOf( ')' ) - name.LastIndexOf( '(' ) - 1 );
			}

			Name = name.Substring( startIndex, endIndex - startIndex );
			Type = type;

			if ( IsArray )
				ItemCount = uint.Parse( name.Substring( name.LastIndexOf( '[' ) + 1, name.LastIndexOf( ']' ) - name.LastIndexOf( '[' ) - 1 ) );

			if ( IsPointer )
				Size = file.Header.PointerSize;
			else
				Size = ( IsPointerArray ? file.Header.PointerSize : Type.Size ) * ItemCount;
		}

		public string Name;
		public readonly Type Type;
		public readonly bool IsFunction;
		public readonly string? FuncArgs;
		public readonly bool IsPointer;
		public readonly int PointerCount;
		public readonly int ItemPointerCount;
		public readonly bool IsPointerArray;
		public readonly bool IsArray;
		public readonly uint Size;
		public readonly uint ItemCount = 1;

		public override string ToString ()
			=> $"{Name}: {(IsFunction ? $"(({FuncArgs}) -> {Type.Name})" : Type.Name )}{(IsPointerArray ? new string( '*', ItemPointerCount ) : "")}{(IsArray ? ($"[{ItemCount}]") : "")}{(IsPointer && !IsFunction ? new string( '*', PointerCount ) : "")}";
	}

	public partial class Structure : Type {
		public Structure ( string name, uint size ) : base( name, size ) {
		}

		public readonly Dictionary<string, Field> Fields = new();

		public override string ToString ()
			=> $"struct {Name} {{ {string.Join( ", ", Fields.Values)} }}";
	}
}
