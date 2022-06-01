namespace osu.Framework.XR.Graphics.Vertices;

public struct UvVertex : IVertex<UvVertex> {
	public float U;
	public float V;

	public UvVertex ( float u, float v ) {
		U = u;
		V = v;
	}

	public UvVertex ( Vector2 vector ) {
		U = vector.X;
		V = vector.Y;
	}

	public int Stride => 2 * sizeof( float );
	public void Link ( Shader shader, int[] attribs ) {
		GL.VertexAttribPointer( attribs[0], 2, VertexAttribPointerType.Float, false, Stride, 0 );
		GL.EnableVertexAttribArray( attribs[0] );
	}
}