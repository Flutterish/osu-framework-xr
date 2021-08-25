namespace osu.Framework.XR.Parsing.Blender.FileBlocks {
	public struct BlendFileBlockHeader {
		public string Identifier;
		public uint Size;
		public ulong OldPointerAddress;
		public uint SDNAIndex;
		public uint Count;
	}
}
