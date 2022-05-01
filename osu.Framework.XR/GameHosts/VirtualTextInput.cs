using osu.Framework.Bindables;
using osu.Framework.Input;
using System;

namespace osu.Framework.XR.GameHosts {
	public class VirtualTextInput : TextInputSource {
		public void AppendText ( string text ) {
			TriggerTextInput( text );
		}

		protected override void ActivateTextInput ( bool allowIme ) {
			IsActiveBindable.Value = true;
			base.ActivateTextInput( allowIme );
		}

		protected override void DeactivateTextInput () {
			IsActiveBindable.Value = false;
			base.DeactivateTextInput();
		}

		public readonly BindableBool IsActiveBindable = new( false );
	}
}
