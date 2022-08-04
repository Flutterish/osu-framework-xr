namespace osu.Framework.XR.Graphics.Transforms;

public class PositionOffsetTransform : PositionTransform {
	public PositionOffsetTransform ( Vector3 offset ) : base( offset ) { }

	protected override void ReadIntoStartValue ( Drawable3D d ) {
		base.ReadIntoStartValue( d );
		EndValue += StartValue;
	}
}
