using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;

namespace osu.Framework.XR.Graphics.Panels;

// TODO wait for o!f to implement (long tap) right click for touch input
public class VirtualTouchHandler : InputHandler {
	public override bool IsActive => true;
	public override bool Initialize ( GameHost host ) => true;

	Dictionary<object, TouchObject> sources = new();
	private void enqueueInput ( IInput input ) {
		PendingInputs.Enqueue( input );
	}

	public void EmulateTouchDown ( object source, Vector2 position ) {
		if ( sources.TryGetValue( source, out var touch ) ) {
			enqueueInput( new TouchInput( touch.Touch, false ) );
			touch.StartPosition = position;
			touch.Position = position;
		}
		else {
			touch = new() {
				StartPosition = position,
				Position = position,
				Index = Enum.GetValues<TouchSource>().Except( sources.Values.Select( x => x.Index ) ).First()
			};
			sources.Add( source, touch );
		}

		enqueueInput( new TouchInput( touch.Touch, true ) );
	}

	public void EmulateTouchMove ( object source, Vector2 position ) {
		if ( !sources.TryGetValue( source, out var touch ) ) {
			EmulateTouchDown( source, position );
			touch = sources[source];
		}

		touch.Position = position;
		enqueueInput( new TouchInput( touch.Touch, true ) );
	}

	public void EmulateTouchUp ( object source ) {
		if ( !sources.Remove( source, out var touch ) )
			return;

		enqueueInput( new TouchInput( touch.Touch, false ) );
	}

	public void ReleaseAllSources () {
		foreach ( var i in sources.Values ) {
			enqueueInput( new TouchInput( i.Touch, false ) );
		}

		sources.Clear();
	}

	private class TouchObject {
		public Vector2 StartPosition;
		public Vector2 Position;
		public TouchSource Index;

		public Touch Touch => new Touch( Index, Position );
	}
}
