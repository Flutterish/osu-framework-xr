using osu.Framework.Platform;

namespace osu.Framework.XR.GameHosts {
	public class VirtualClipboard : Clipboard {
		string copied;
		public override string GetText () {
			return copied;
		}

		public override void SetText ( string selectedText ) {
			copied = selectedText;
		}
	}
}
