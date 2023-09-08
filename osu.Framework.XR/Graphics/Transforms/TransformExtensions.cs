using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Framework.XR.Graphics.Transforms;

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

	public static TransformSequence<T> ScaleTo<T> ( this T drawable, float scale, double duration = 0, Easing easing = Easing.None )
		where T : Drawable3D
		=> drawable.TransformTo( drawable.PopulateTransform( new ScaleTransform( new Vector3( scale ) ), default, duration, new DefaultEasingFunction( easing ) ) );
	/// <summary>
	/// This is not a generic transform extensions as the generic version causes an ambiguious reference error, use the Vector3 scale for a generic transform sequence.
	/// </summary>
	public static TransformSequence<Drawable3D> ScaleTo ( this Drawable3D drawable, float scale, double duration = 0, Easing easing = Easing.None )
		=> drawable.TransformTo( drawable.PopulateTransform( new ScaleTransform( new Vector3( scale ) ), default, duration, new DefaultEasingFunction( easing ) ) );
	public static TransformSequence<T> ScaleTo<T> ( this TransformSequence<T> seq, float scale, double duration = 0, Easing easing = Easing.None )
		where T : Drawable3D
		=> seq.Append( o => o.ScaleTo( scale, duration, easing ) );

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
}
