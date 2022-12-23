namespace osu.Framework.XR.Maths;

public struct Plane {
	public Vector3 Origin;
	public Vector3 Normal;

	public float DistanceTo ( Vector3 point )
		=> Math.Abs( Normal.Dot( point - Origin ) );
}
