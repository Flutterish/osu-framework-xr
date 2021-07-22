using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;
using osuTK;

namespace osu.Framework.XR.Components {
	public static class TransformExtensions {
		public static TransformSequence<T> MoveTo<T> ( this T drawable, Vector3 position, double duration = 0, Easing easing = Easing.None )
			where T : Drawable3D
			=> drawable.TransformTo( drawable.PopulateTransform( new PositionTransform( position ), default, duration, new DefaultEasingFunction( easing ) ) );
		public static TransformSequence<T> MoveTo<T> ( this TransformSequence<T> seq, Vector3 position, double duration = 0, Easing easing = Easing.None )
			where T : Drawable3D
			=> seq.Append( o => o.MoveTo( position, duration, easing ) );
		public static TransformSequence<T> MoveToOffset<T> ( this T drawable, Vector3 offset, double duration = 0, Easing easing = Easing.None )
			where T : Drawable3D
			=> drawable.TransformTo( drawable.PopulateTransform( new PositionOffsetTransform( offset ), default, duration, new DefaultEasingFunction( easing ) ) );
		public static TransformSequence<T> MoveToOffset<T> ( this TransformSequence<T> seq, Vector3 offset, double duration = 0, Easing easing = Easing.None )
			where T : Drawable3D
			=> seq.Append( o => o.MoveToOffset( offset, duration, easing ) );

		public static TransformSequence<T> ScaleTo<T> ( this T drawable, Vector3 scale, double duration = 0, Easing easing = Easing.None )
			where T : Drawable3D
			=> drawable.TransformTo( drawable.PopulateTransform( new ScaleTransform( scale ), default, duration, new DefaultEasingFunction( easing ) ) );
		public static TransformSequence<T> ScaleTo<T> ( this TransformSequence<T> seq, Vector3 scale, double duration = 0, Easing easing = Easing.None )
			where T : Drawable3D
			=> seq.Append( o => o.ScaleTo( scale, duration, easing ) );

		public static TransformSequence<T> RotateTo<T> ( this T drawable, Quaternion rotation, double duration = 0, Easing easing = Easing.None )
			where T : Drawable3D
			=> drawable.TransformTo( drawable.PopulateTransform( new RotationTransform( rotation ), default, duration, new DefaultEasingFunction( easing ) ) );
		public static TransformSequence<T> RotateTo<T> ( this TransformSequence<T> seq, Quaternion rotation, double duration = 0, Easing easing = Easing.None )
			where T : Drawable3D
			=> seq.Append( o => o.RotateTo( rotation, duration, easing ) );

		private class PositionTransform : Transform<Vector3, Drawable3D> {
			private readonly Vector3 target;

			public override string TargetMember => nameof( Drawable3D.Position );

			public PositionTransform ( Vector3 target ) {
				this.target = target;
			}

			private Vector3 positionAt ( double time ) {
				if ( time < StartTime ) return StartValue;
				if ( time >= EndTime ) return EndValue;

				return StartValue + Interpolation.ValueAt( time, 0f, 1f, StartTime, EndTime, Easing ) * ( EndValue - StartValue );
			}

			protected override void Apply ( Drawable3D d, double time ) => d.Position = positionAt( time );

			protected override void ReadIntoStartValue ( Drawable3D d ) {
				StartValue = d.Position;
				EndValue = target;
			}
		}

		private class PositionOffsetTransform : PositionTransform {
			public PositionOffsetTransform ( Vector3 offset ) : base( offset ) {
			}

			protected override void ReadIntoStartValue ( Drawable3D d ) {
				base.ReadIntoStartValue( d );
				EndValue += StartValue;
			}
		}

		private class ScaleTransform : Transform<Vector3, Drawable3D> {
			private readonly Vector3 target;

			public override string TargetMember => nameof( Drawable3D.Scale );

			public ScaleTransform ( Vector3 target ) {
				this.target = target;
			}

			private Vector3 scaleAt ( double time ) {
				if ( time < StartTime ) return StartValue;
				if ( time >= EndTime ) return EndValue;

				return StartValue + Interpolation.ValueAt( time, 0f, 1f, StartTime, EndTime, Easing ) * ( EndValue - StartValue );
			}

			protected override void Apply ( Drawable3D d, double time ) => d.Scale = scaleAt( time );

			protected override void ReadIntoStartValue ( Drawable3D d ) {
				StartValue = d.Scale;
				EndValue = target;
			}
		}

		private class RotationTransform : Transform<Quaternion, Drawable3D> {
			private readonly Quaternion target;

			public override string TargetMember => nameof( Drawable3D.Rotation );

			public RotationTransform ( Quaternion target ) {
				this.target = target;
			}

			private Quaternion rotationAt ( double time ) {
				if ( time < StartTime ) return StartValue;
				if ( time >= EndTime ) return EndValue;

				return Quaternion.Slerp( StartValue, EndValue, Interpolation.ValueAt( time, 0f, 1f, StartTime, EndTime, Easing ) );
			}

			protected override void Apply ( Drawable3D d, double time ) => d.Rotation = rotationAt( time );

			protected override void ReadIntoStartValue ( Drawable3D d ) {
				StartValue = d.Rotation;
				EndValue = target;
			}
		}
	}
}
