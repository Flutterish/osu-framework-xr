using osu.Framework.Bindables;
using osu.Framework.XR.Graphics.Panels;
using osuTK.Input;

namespace osu.Framework.XR.Input;

/// <summary>
/// A system which manages panel input and focus though independent input sources
/// </summary>
public class PanelInteractionSystem {
	Stack<HashSet<Source>> hashSetPool = new();
	Dictionary<Panel, HashSet<Source>> focusedPanels = new();

	void focus ( Source source, Panel panel ) {
		if ( !focusedPanels.TryGetValue( panel, out var sources ) ) {
			if ( !hashSetPool.TryPop( out sources ) )
				sources = new();

			focusedPanels.Add( panel, sources );
			panel.Content.HasFocus = true;
			PanelFocused?.Invoke( panel );
		}

		sources.Add( source );
	}
	void blur ( Source source, Panel panel ) {
		var sources = focusedPanels[panel];
		sources.Remove( source );

		if ( sources.Count == 0 ) {
			hashSetPool.Push( sources );
			focusedPanels.Remove( panel );
			panel.Content.HasFocus = false;
			PanelBlurred?.Invoke( panel );
		}
	}

	Dictionary<object, Source> sources = new();
	public Source GetSource ( object source ) {
		if ( !sources.TryGetValue( source, out var v ) )
			sources.Add( sources, v = new( this ) );

		return v;
	}

	public event Action<Panel>? PanelFocused;
	public event Action<Panel>? PanelBlurred;

	public class Source {
		public readonly Bindable<Panel?> FocusedPanelBindable = new();
		public Panel? FocusedPanel {
			get => FocusedPanelBindable.Value;
			set => FocusedPanelBindable.Value = value;
		}

		public Source ( PanelInteractionSystem system ) {
			FocusedPanelBindable.BindValueChanged( v => {
				if ( v.OldValue is Panel old ) {
					releaseInput( old );
					system.blur( this, old );
				}

				if ( v.NewValue is Panel panel )
					system.focus( this, panel );
			} );
		}

		HashSet<Key> pressedKeys = new();
		HashSet<MouseButton> pressedButtons = new();
		void releaseInput ( Panel panel ) {
			panel.Content.TouchUp( this );
			foreach ( var i in pressedKeys )
				panel.Content.Release( i );
			pressedKeys.Clear();
			foreach ( var i in pressedButtons )
				panel.Content.Release( i );
			pressedButtons.Clear();
		}

		public void ReleaseKeyboard () {
			if ( FocusedPanel is null )
				return;

			foreach ( var i in pressedKeys )
				FocusedPanel.Content.Release( i );
			pressedKeys.Clear();
		}

		public void ReleaseMouse () {
			if ( FocusedPanel is null )
				return;

			foreach ( var i in pressedButtons )
				FocusedPanel.Content.Release( i );
			pressedButtons.Clear();
		}

		public void Press ( Key key ) {
			if ( FocusedPanel is null )
				return;

			if ( pressedKeys.Add( key ) )
				FocusedPanel.Content.Press( key );
		}
		public void Release ( Key key ) {
			if ( FocusedPanel is null )
				return;

			if ( pressedKeys.Remove( key ) )
				FocusedPanel.Content.Release( key );
		}

		public void Press ( MouseButton button ) {
			if ( FocusedPanel is null )
				return;

			if ( pressedButtons.Add( button ) )
				FocusedPanel.Content.Press( button );
		}
		public void Release ( MouseButton key ) {
			if ( FocusedPanel is null )
				return;

			if ( pressedButtons.Remove( key ) )
				FocusedPanel.Content.Release( key );
		}

		public void Scroll ( Vector2 delta ) {
			if ( FocusedPanel != null )
				FocusedPanel.Content.Scroll += delta;
		}

		public void MoveMouse ( Vector2 pos ) {
			FocusedPanel?.Content.MoveMouse( pos );
		}

		public void TouchDown ( Vector2 pos ) {
			FocusedPanel?.Content.TouchDown( this, pos );
		}
		public void TouchMove ( Vector2 pos ) {
			FocusedPanel?.Content.TouchMove( this, pos );
		}
		public void TouchUp () {
			FocusedPanel?.Content.TouchUp( this );
		}
	}
}