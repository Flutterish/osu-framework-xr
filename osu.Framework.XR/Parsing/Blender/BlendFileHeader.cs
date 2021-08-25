namespace osu.Framework.XR.Parsing.Blender {
	public struct BlendFileHeader {
		/// <summary>
		/// Always "BLENDER". If it is not, something is wrong.
		/// </summary>
		public string Identifier;
		/// <summary>
		/// Either 4 (32 bit) or 8 (64 bit)
		/// </summary>
		public uint PointerSize;
		public bool IsLittleEndian;
		public string Version;
	}
}
