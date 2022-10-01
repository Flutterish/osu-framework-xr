using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;

namespace osu.Framework.XR.Graphics.Transforms;
public class ScaleTransform : Transform<Vector3, Drawable3D> {
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
