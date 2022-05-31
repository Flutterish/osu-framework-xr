using osu.Framework.XR.Graphics.Buffers;

namespace osu.Framework.XR.Graphics;

public class Mesh {
	public IReadOnlyList<IVertexBuffer> VertexBuffers => vertexBuffers;
	readonly IVertexBuffer[] vertexBuffers;
	public readonly IElementBuffer ElementBuffer;

	public Mesh ( IElementBuffer elementBuffer, params IVertexBuffer[] vertexBuffers ) {
		ElementBuffer = elementBuffer;
		this.vertexBuffers = new IVertexBuffer[ vertexBuffers.Length ];
		Array.Copy( vertexBuffers, this.vertexBuffers, vertexBuffers.Length );
	}
}