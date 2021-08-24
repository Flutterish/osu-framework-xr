using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing.Blender.FileBlocks {
	public abstract class BlendFileBlock {
		public BlendFileBlockHeader Header;

		protected BlendFileBlock ( BlendFileBlockHeader header ) {
			Header = header;
		}
	}
}
