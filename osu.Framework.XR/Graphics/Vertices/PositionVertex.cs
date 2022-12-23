namespace osu.Framework.XR.Graphics.Vertices;

public struct PositionVertex : IVertex<PositionVertex> {
	public float X;
	public float Y;
	public float Z;

	public PositionVertex ( float x, float y, float z ) {
		X = x;
		Y = y;
		Z = z;
	}

	public PositionVertex ( Vector3 vector ) {
		X = vector.X;
		Y = vector.Y;
		Z = vector.Z;
	}

	public int Stride => 3 * sizeof( float );
	public void Link ( Span<int> attribs ) {
		GL.VertexAttribPointer( attribs[0], 3, VertexAttribPointerType.Float, false, Stride, 0 );
		GL.EnableVertexAttribArray( attribs[0] );
	}

	public static implicit operator Vector3 ( PositionVertex vertex )
		=> new( vertex.X, vertex.Y, vertex.Z );
}