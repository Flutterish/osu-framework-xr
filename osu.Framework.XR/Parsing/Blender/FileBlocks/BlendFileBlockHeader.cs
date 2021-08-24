using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing.Blender.FileBlocks {
	public struct BlendFileBlockHeader {
		public string Identifier;
		public uint Size;
		public ulong OldPointerAddress;
		public uint SDNA;
		public uint Count;
	}
}
