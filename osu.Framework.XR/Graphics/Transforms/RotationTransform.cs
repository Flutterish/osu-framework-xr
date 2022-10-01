using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;

namespace osu.Framework.XR.Graphics.Transforms;

public class RotationTransform : Transform<Quaternion, Drawable3D> {
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
