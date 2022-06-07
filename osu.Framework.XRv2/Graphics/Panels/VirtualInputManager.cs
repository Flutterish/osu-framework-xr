using osu.Framework.Input;

namespace osu.Framework.XR.Graphics.Panels;

public class VirtualInputManager : CustomInputManager {
	protected readonly VirtualMouseHandler Mouse;
	public VirtualInputManager () {
		AddHandler( Mouse = new() );
	}

	public void MoveMouse ( Vector2 position )
		=> Mouse.EmulateMouseMove( position );
}
