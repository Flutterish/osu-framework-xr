using osu.Framework.Graphics;
using osu.Framework.Input;
using osuTK.Input;

namespace osu.Framework.XR.Graphics.Panels;

public partial class VirtualInputManager : CustomInputManager {
	protected readonly VirtualMouseHandler Mouse;
	protected readonly VirtualKeyboardHandler Keyboard;
	protected readonly VirtualTouchHandler Touch;
	public VirtualInputManager () {
		AddHandler( Mouse = new() );
		AddHandler( Keyboard = new() );
		AddHandler( Touch = new() );

		AddInternal( focusLock = new() );
	}

	FocusLock focusLock;
	bool hasFocus = false;
	new public bool HasFocus {
		get => hasFocus;
		set {
			hasFocus = value;
			if ( !hasFocus ) {
				ReleaseAllInput();
				ChangeFocus( focusLock );
			}
		}
	}
	public override bool HandleHoverEvents => HasFocus;

	protected override void Update () {
		base.Update();
		if ( !hasFocus && FocusedDrawable != focusLock )
			ChangeFocus( focusLock );
	}

	public virtual void ReleaseAllInput () {
		ReleaseKeyboardInput();
		ReleaseMouseInput();
		ReleaseTouchInput();
	}

	public void ReleaseKeyboardInput () {
		foreach ( var i in pressedKeys )
			Keyboard.EmulateKeyUp( i );

		pressedKeys.Clear();
	}

	public void ReleaseMouseInput () {
		foreach ( var i in pressedButtons )
			Mouse.EmulateMouseUp( i );

		pressedButtons.Clear();
	}

	public void ReleaseTouchInput () {
		Touch.ReleaseAllSources();
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

	public void TouchDown ( object source, Vector2 position )
		=> Touch.EmulateTouchDown( source, position );

	public void TouchUp ( object source )
		=> Touch.EmulateTouchUp( source );

	public void TouchMove ( object source, Vector2 position )
		=> Touch.EmulateTouchMove( source, position );

	partial class FocusLock : Drawable {
		public FocusLock () {
			AlwaysPresent = true;
		}

		public override bool AcceptsFocus => true;
	}
}
