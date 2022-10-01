namespace osu.Framework.XR.Graphics.Vertices;

public struct TexturedNormal : IVertex<TexturedNormal> {
	public Vector3 Position;
	public Vector2 UV;
	public Vector3 Normal;

	public int Stride => 8 * sizeof( float );
	public void Link ( Span<int> attribs ) {
		GL.VertexAttribPointer( attribs[0], 3, VertexAttribPointerType.Float, false, Stride, 0 );
		GL.EnableVertexAttribArray( attribs[0] );
		GL.VertexAttribPointer( attribs[1], 2, VertexAttribPointerType.Float, false, Stride, 3 * sizeof( float ) );
		GL.EnableVertexAttribArray( attribs[1] );
		GL.VertexAttribPointer( attribs[2], 3, VertexAttribPointerType.Float, false, Stride, 5 * sizeof( float ) );
		GL.EnableVertexAttribArray( attribs[2] );
	}
}
