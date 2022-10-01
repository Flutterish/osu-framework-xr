namespace osu.Framework.XR.Maths;

public struct Face {
	public Vector3 A;
	public Vector3 B;
	public Vector3 C;

	public Face ( Vector3 a, Vector3 b, Vector3 c ) {
		A = a;
		B = b;
		C = c;
	}

	public Vector3 Centre
		=> ( A + B + C ) / 3;

	public static Face operator * ( Matrix4 matrix, Face face ) {
		return new Face(
			( new Vector4( face.A, 1 ) * matrix ).Xyz,
			( new Vector4( face.B, 1 ) * matrix ).Xyz,
			( new Vector4( face.C, 1 ) * matrix ).Xyz
		);
	}
}