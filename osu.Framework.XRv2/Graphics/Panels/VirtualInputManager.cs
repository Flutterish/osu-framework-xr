using osu.Framework.Input;
using osuTK.Input;

namespace osu.Framework.XR.Graphics.Panels;

public class VirtualInputManager : CustomInputManager {
	protected readonly VirtualMouseHandler Mouse;
	protected readonly VirtualKeyboardHandler Keyboard;
	// TODO touch handlers
	public VirtualInputManager () {
		AddHandler( Mouse = new() );
		AddHandler( Keyboard = new() );
	}

	public void ReleaseAllInput () {
		foreach ( var i in pressedButtons )
			Mouse.EmulateMouseUp( i );

		pressedButtons.Clear();

		foreach ( var i in pressedKeys )
			Keyboard.EmulateKeyUp( i );

		pressedKeys.Clear();
	}

	public void MoveMouse ( Vector2 position )
		=> Mouse.EmulateMouseMove( position );

	HashSet<MouseButton> pressedButtons = new();
	public void Press ( MouseButton button ) {
		if ( pressedButtons.Contains( button ) )
			return;

		pressedButtons.Add( button );
		Mouse.EmulateMouseDown( button );
	}

	public void Release ( MouseButton button ) {
		if ( !pressedButtons.Contains( button ) )
			return;

		pressedButtons.Remove( button );
		Mouse.EmulateMouseUp( button );
	}

	public bool AllowScroll = true;
	private Vector2 scroll;
	public Vector2 Scroll {
		get => scroll;
		set {
			if ( AllowScroll ) Mouse.EmulateMouseWheel( value - scroll, false );
			scroll = value;
		}
	}

	HashSet<Key> pressedKeys = new();
	public void Press ( Key key ) {
		if ( pressedKeys.Contains( key ) )
			return;

		pressedKeys.Add( key );
		Keyboard.EmulateKeyDown( key );
	}

	public void Release ( Key key ) {
		if ( !pressedKeys.Contains( key ) )
			return;

		pressedKeys.Remove( key );
		Keyboard.EmulateKeyUp( key );
	}
}
