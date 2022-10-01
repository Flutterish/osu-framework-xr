using osu.Framework.Input.Handlers;
using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;
using osuTK.Input;

namespace osu.Framework.XR.Graphics.Panels;

public class VirtualMouseHandler : InputHandler {
	public override bool IsActive => true;
	public override bool Initialize ( GameHost host ) => true;

	private void enqueueInput ( IInput input ) {
		PendingInputs.Enqueue( input );
	}

	public void EmulateMouseMove ( Vector2 position ) => enqueueInput( new MousePositionAbsoluteInput { Position = position } );
	public void EmulateMouseDown ( MouseButton button ) => enqueueInput( new MouseButtonInput( button, true ) );
	public void EmulateMouseUp ( MouseButton button ) => enqueueInput( new MouseButtonInput( button, false ) );
	public void EmulateMouseWheel ( Vector2 delta, bool precise ) => enqueueInput( new MouseScrollRelativeInput { Delta = delta, IsPrecise = precise } );
}
