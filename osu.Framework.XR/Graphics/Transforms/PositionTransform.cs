using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;

namespace osu.Framework.XR.Graphics.Transforms;

public class PositionTransform : Transform<Vector3, Drawable3D> {
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
