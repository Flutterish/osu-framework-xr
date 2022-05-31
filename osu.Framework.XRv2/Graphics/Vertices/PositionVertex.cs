﻿namespace osu.Framework.XR.Graphics.Vertices;

public struct PositionVertex : IVertex<PositionVertex> {
	public Vector3 Position;

	public int Stride => 3 * sizeof( float );
	public void Link ( Shader shader, int[] attribs ) {
		GL.VertexAttribPointer( attribs[0], 3, VertexAttribPointerType.Float, false, Stride, 0 );
		GL.EnableVertexAttribArray( attribs[0] );
	}
}