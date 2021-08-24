using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing.Blender.FileBlocks {
	public class UnidentifiedBlendFileBlock : BlendFileBlock {
		public UnidentifiedBlendFileBlock ( BlendFileBlockHeader header, BlendFile file, Stream stream ) : base( header ) {
			RawData = new byte[ header.Size ];
			stream.EnsureRead( RawData, 0, (int)header.Size );
		}

		public byte[] RawData;
	}
}
