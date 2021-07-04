using osu.Framework.Input.Handlers;
using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;
using TKKey = osuTK.Input.Key;

namespace osu.Framework.XR.Input {
	public class VirtualKeyboardHandler : InputHandler {
		public override bool IsActive => true;
		public override bool Initialize ( GameHost host ) => true;

		private void enqueueInput ( IInput input ) {
			PendingInputs.Enqueue( input );
		}

		public void EmulateKeyDown ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, true ) );
		public void EmulateKeyUp ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, false ) );
	}
}
