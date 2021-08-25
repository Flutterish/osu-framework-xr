using System.IO;

namespace osu.Framework.XR.Parsing.Blender.FileBlocks {
	public abstract class BlendFileBlock {
		public BlendFileBlockHeader Header;

		protected BlendFileBlock ( BlendFileBlockHeader header ) {
			Header = header;
		}

		public abstract void PostProcess ( BlendFile file, SDNABlock sdna, Stream stream );
	}
}
