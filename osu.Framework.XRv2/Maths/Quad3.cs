namespace osu.Framework.XR.Maths;

public struct Quad3 {
	public Vector3 TL;
	public Vector3 TR;
	public Vector3 BL;
	public Vector3 BR;

	public Quad3 ( Vector3 tL, Vector3 tR, Vector3 bL, Vector3 bR ) {
		TL = tL;
		TR = tR;
		BL = bL;
		BR = bR;
	}

	public static Quad3 operator + ( Quad3 quad, Vector3 offset )
		=> new Quad3( quad.TL + offset, quad.TR + offset, quad.BL + offset, quad.BR + offset );
}