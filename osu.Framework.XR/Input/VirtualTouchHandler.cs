using osu.Framework.Bindables;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Input {
	public class VirtualTouchHandler : InputHandler {
		public override bool Initialize ( GameHost host ) => true;
		public override bool IsActive => true;

		public readonly BindableInt DeadzoneBindable = new( 20 );
		//double holdDuration = 500;

		//[BackgroundDependencyLoader]
		//private void load ( XrConfigManager config ) {
		//	config.BindWith( XrConfigSetting.Deadzone, DeadzoneBindable );
		//}

		private void enqueueInput ( IInput input ) {
			PendingInputs.Enqueue( input );
		}

		Dictionary<object, TouchObject> sources = new();
		public void EmulateTouchDown ( object source, Vector2 position, double time ) {
			var touch = new TouchObject { LastUpdateTime = time, StartTime = time, Position = position, StartPosition = position, Index = Enum.GetValues<TouchSource>().Except( sources.Select( x => x.Value.Index ) ).First() };
			sources.Add( source, touch );
			enqueueInput( new TouchInput( touch.Touch, true ) );
		}

		public void EmulateTouchMove ( object source, Vector2 position, double time ) {
			if ( !sources.ContainsKey( source ) ) return;

			var touch = sources[ source ];
			touch.Position = position;
			touch.LastUpdateTime = time;
			if ( ( touch.StartPosition - position ).Length > DeadzoneBindable.Value )
				touch.InDeadzone = false;

			if ( !touch.InDeadzone )
				enqueueInput( new TouchInput( touch.Touch, true ) ); // drag
		}

		public void EmulateTouchUp ( object source, double time ) {
			if ( !sources.ContainsKey( source ) ) return;

			var touch = sources[ source ];
			sources.Remove( source );
			enqueueInput( new TouchInput( touch.Touch, false ) );
			//if ( !touch.RightClick )
			//	enqueueInput( new TouchInput( touch.Touch, false ) ); // tap if in deadzone
			//else {
			//	touch.Position += new Vector2( 50 );
			//	enqueueInput( new TouchInput( touch.Touch, true ) );
			//	touch.Position = touch.StartPosition;
			//	enqueueInput( new TouchInput( touch.Touch, true ) );
			//	enqueueInput( new TouchInput( touch.Touch, false ) );
			//
			//	PendingInputs.Enqueue( new MousePositionAbsoluteInput { Position = touch.StartPosition } );
			//	PendingInputs.Enqueue( new MouseButtonInput( MouseButton.Right, true ) );
			//	PendingInputs.Enqueue( new MouseButtonInput( MouseButton.Right, false ) );
			//}
		}

		public void ReleaseAllSources ( double time ) {
			foreach ( var i in sources.ToArray() ) {
				EmulateTouchUp( i.Key, time );
			}
		}

		public void Update ( double time ) {
			//foreach ( var i in sources ) {
			//	var touch = i.Value;
			//	if ( touch.InDeadzone ) {
			//		if ( time - touch.StartTime >= holdDuration ) {
			//			// hold
			//			touch.RightClick = true;
			//		}
			//	}
			//}
			// TODO right click only in menu
		}

		private class TouchObject {
			public Vector2 StartPosition;
			public Vector2 Position;
			public double StartTime;
			public double LastUpdateTime;
			public bool InDeadzone = true;
			public TouchSource Index;
			//public bool RightClick;

			public Touch Touch => new Touch( Index, Position );
		}
	}
}
