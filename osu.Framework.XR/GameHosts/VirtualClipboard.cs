using osu.Framework.Platform;
using SixLabors.ImageSharp;

namespace osu.Framework.XR.GameHosts {
	public class VirtualClipboard : Clipboard {
		string copied = string.Empty;
		public override string GetText () {
			return copied;
		}

		public override void SetText ( string selectedText ) {
			copied = selectedText;
		}

		public override Image<TPixel> GetImage<TPixel> () {
			throw new System.NotImplementedException();
		}

		public override bool SetImage ( Image image ) {
			return false;
		}
	}
}
