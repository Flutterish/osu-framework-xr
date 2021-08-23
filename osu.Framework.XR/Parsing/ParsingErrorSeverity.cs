using System;

namespace osu.Framework.XR.Parsing {
	[Flags]
	public enum ParsingErrorSeverity {
		Success = 0,
		Issue = 1,
		Error = 2,
		NotImplemented = 4
	}
}
