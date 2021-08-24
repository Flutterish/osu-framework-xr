using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing.Blender.FileBlocks {
	public class SDNABlock : BlendFileBlock {
		public string[] Names;
		public string[] Types;
		public ushort[] TypeSizes;
		public Structure[] Structs;

		public struct Structure {
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
				Names = new string[ count ];
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
				Types = new string[ count ];
				for ( int i = 0; i < count; i++ ) {
					Types[ i ] = file.ParseCString( stream );
				}

				stream.Align( 4 );
				stream.EnsureRead( buffer, 0, 4 );
				identifier = file.ParseString( buffer, 0, 4 );
				if ( identifier != "TLEN" )
					throw new InvalidDataException( $"Expected 4th identifier in DNA1 block to be 'TLEN' but it was '{identifier}'" );

				TypeSizes = new ushort[ count ];
				for ( int i = 0; i < count; i++ ) {
					stream.EnsureRead( buffer, 0, 2 );
					TypeSizes[ i ] = file.ParseU16( buffer, 0 );
				}

				stream.Align( 4 );
				stream.EnsureRead( buffer, 0, 4 );
				identifier = file.ParseString( buffer, 0, 4 );
				if ( identifier != "STRC" )
					throw new InvalidDataException( $"Expected 5th identifier in DNA1 block to be 'STRC' but it was '{identifier}'" );

				stream.EnsureRead( buffer, 0, 4 );
				count = file.ParseU32( buffer, 0 );
				Structs = new Structure[count];
				for ( int i = 0; i < count; i++ ) {
					Structure s = new();
					stream.EnsureRead( buffer, 0, 4 );
					s.Type = file.ParseU16( buffer, 0 );
					var fieldCount = file.ParseU16( buffer, 2 );
					s.Fields = new (ushort Type, ushort Name)[fieldCount];
					for ( int k = 0; k < fieldCount; k++ ) {
						stream.EnsureRead( buffer, 0, 4 );
						s.Fields[ k ] = ( file.ParseU16( buffer, 0 ), file.ParseU16( buffer, 2 ) );
					}
				}
			}
			finally {
				ArrayPool<byte>.Shared.Return( buffer );
			}
		}
	}
}
