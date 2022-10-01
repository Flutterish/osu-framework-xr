using osu.Framework.Input.Handlers;
using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;
using osuTK.Input;

namespace osu.Framework.XR.Graphics.Panels;

public class VirtualKeyboardHandler : InputHandler {
	public override bool IsActive => true;
	public override bool Initialize ( GameHost host ) => true;

	private void enqueueInput ( IInput input ) {
		PendingInputs.Enqueue( input );
	}

	public void EmulateKeyDown ( Key key ) => enqueueInput( new KeyboardKeyInput( key, true ) );
	public void EmulateKeyUp ( Key key ) => enqueueInput( new KeyboardKeyInput( key, false ) );
}