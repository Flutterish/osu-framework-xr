using System;

namespace osu.Framework.XR.Parsing.WaveFront {
	[Flags]
	public enum OBJParsingErrorSeverity {
		Success = 0,
		Issue = 1,
		Error = 2,
		NotImplemented = 4
	}
}
