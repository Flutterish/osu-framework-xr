using osu.Framework.Bindables;
using osu.Framework.Input;
using System;

namespace osu.Framework.XR.GameHosts {
	public class VirtualTextInput : ITextInputSource {
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

		public void Deactivate ( object sender ) {
			pending = "";
			IsActiveBindable.Value = false;
		}

		public void Activate ( object sender ) {
			pending = "";
			IsActiveBindable.Value = true;
		}

		public readonly BindableBool IsActiveBindable = new( false );

		public bool ImeActive => false;

		public event Action<string> OnNewImeComposition;
		public event Action<string> OnNewImeResult;
	}
}
