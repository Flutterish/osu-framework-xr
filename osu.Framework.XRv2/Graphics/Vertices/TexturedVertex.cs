namespace osu.Framework.XR.Graphics.Vertices;

public struct TexturedVertex : IVertex<TexturedVertex> {
	public Vector3 Position;
	public Vector2 UV;

	public int Stride => 5 * sizeof( float );
	public void Link ( Shader shader, int[] attribs ) {
		GL.VertexAttribPointer( attribs[0], 3, VertexAttribPointerType.Float, false, Stride, 0 );
		GL.EnableVertexAttribArray( attribs[0] );
		GL.VertexAttribPointer( attribs[1], 2, VertexAttribPointerType.Float, false, Stride, 3 * sizeof( float ) );
		GL.EnableVertexAttribArray( attribs[1] );
	}
}