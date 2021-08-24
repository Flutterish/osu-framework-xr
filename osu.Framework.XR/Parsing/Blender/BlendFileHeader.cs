using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing.Blender {
	public struct BlendFileHeader {
		/// <summary>
		/// Always "BLENDER". If it is not, something is wrong.
		/// </summary>
		public string Identifier;
		/// <summary>
		/// Either 4 (32 bit) or 8 (64 bit)
		/// </summary>
		public int PointerSize;
		public bool IsLittleEndian;
		public string Version;
	}
}
