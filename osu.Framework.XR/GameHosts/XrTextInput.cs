using osu.Framework.Input;
using System;

namespace osu.Framework.XR.GameHosts {
	public class XrTextInput : ITextInputSource {
		private string pending;

		public void AppendText ( string text ) {
			pending += text;
		}

		public string GetPendingText () {
			try {
				return pending;
			}
			finally {
				pending = string.Empty;
			}
		}

		public void Deactivate ( object sender ) { }

		public void Activate ( object sender ) { }

		public bool ImeActive => false;

		public event Action<string> OnNewImeComposition;
		public event Action<string> OnNewImeResult;
	}
}
