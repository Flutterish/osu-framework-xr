using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Framework.Utils;
using osuTK;
using osuTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TKKey = osuTK.Input.Key;

namespace osu.Framework.XR.Input {
	/// <summary>
	/// XR input is passed to 2D drawables though this manger.
	/// </summary>
	public partial class VirtualInputManager : CustomInputManager {
		public readonly VirtualMouseHandler Mouse;
		public readonly VirtualKeyboardHandler Keyboard;
		public readonly VirtualTouchHandler Touch;

		public VirtualInputManager () {
			AddHandler( Mouse = new VirtualMouseHandler() );
			AddHandler( Keyboard = new VirtualKeyboardHandler() );
			AddHandler( Touch = new VirtualTouchHandler() );
		}

		public Drawable? GetFirstDrawableAt ( Vector2 position ) {
			return firstDrawableAt( this, position, ScreenSpaceDrawQuad );
		}

		static readonly MethodInfo getAliveInternalChildrenMethod = typeof( CompositeDrawable ).GetProperty( nameof( AliveInternalChildren ), BindingFlags.NonPublic | BindingFlags.Instance )!.GetGetMethod( nonPublic: true )!;
		static readonly Func<CompositeDrawable, IReadOnlyList<Drawable>> getAliveInternalChildren = x => (getAliveInternalChildrenMethod.Invoke( x, Array.Empty<object>() ) as IReadOnlyList<Drawable>)!;
		Drawable? firstDrawableAt ( Drawable parent, Vector2 position, Quad mask ) {
			if ( parent is CompositeDrawable composite ) {
				foreach ( var i in getAliveInternalChildren( composite ).Reverse() ) {
					if ( !i.IsPresent || i is Component )
						continue;
					if ( i.AlwaysPresent && Precision.AlmostEquals( i.Alpha, 0f ) )
						continue;

					var drawable = firstDrawableAt( i, position, composite.Masking || composite is BufferedContainer<Drawable> ? composite.ScreenSpaceDrawQuad : mask );
					if ( drawable is not null ) return drawable;
				}

				return null;
			}
			else {
				if ( parent.ScreenSpaceDrawQuad.Contains( position ) && mask.Contains( position ) ) return parent;
				return null;
			}
		}

		new public bool HasFocus;
		public override bool HandleHoverEvents => HasFocus;

		protected override void Update () {
			base.Update();
			Touch.Update( Time.Current );
		}

		private bool isLeftPressed = false;
		public bool IsLeftPressed {
			get => isLeftPressed;
			set {
				if ( isLeftPressed == value ) return;
				isLeftPressed = value;
				if ( isLeftPressed )
					Mouse.EmulateMouseDown( MouseButton.Left );
				else
					Mouse.EmulateMouseUp( MouseButton.Left );
			}
		}
		private bool isRightPressed = false;
		public bool IsRightPressed {
			get => isRightPressed;
			set {
				if ( isRightPressed == value ) return;
				isRightPressed = value;
				if ( isRightPressed )
					Mouse.EmulateMouseDown( MouseButton.Right );
				else
					Mouse.EmulateMouseUp( MouseButton.Right );
			}
		}
		private Vector2 scroll;
		public Vector2 Scroll {
			get => scroll;
			set {
				Mouse.EmulateMouseWheel( value - scroll, false );
				scroll = value;
			}
		}

		public void TouchDown ( object source, Vector2 position ) {
			Touch.EmulateTouchDown( source, position, Time.Current );
		}
		public void TouchMove ( object source, Vector2 position ) {
			Touch.EmulateTouchMove( source, position, Time.Current );
		}
		public void TouchUp ( object source ) {
			Touch.EmulateTouchUp( source, Time.Current );
		}
		public void ReleaseAllTouch () {
			Touch.ReleaseAllSources( Time.Current );
		}

		public void PressKey ( TKKey key ) {
			Keyboard.EmulateKeyDown( key );
			Keyboard.EmulateKeyUp( key );
		}
		public void HoldKey ( TKKey key ) {
			Keyboard.EmulateKeyDown( key );
		}
		public void ReleaseKey ( TKKey key ) {
			Keyboard.EmulateKeyUp( key );
		}
	}
}
