using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics.Buffers;

namespace osu.Framework.XR.Graphics.Meshes;

/// <summary>
/// Geometry data defined by one or more vertice buffers and an optional element buffer.
/// </summary>
/// <remarks>
/// Note that drawing a mesh requires a linked <see cref="IAttributeArray"/> to be bound
/// </remarks>
public class Mesh : IDisposable {
	public MeshDescriptor? Descriptor;
	public IReadOnlyList<IVertexBuffer> VertexBuffers => vertexBuffers;
	readonly IVertexBuffer[] vertexBuffers;
	public readonly IElementBuffer? ElementBuffer;

	public Mesh ( IElementBuffer? elementBuffer, params IVertexBuffer[] vertexBuffers ) {
		ElementBuffer = elementBuffer;
		this.vertexBuffers = new IVertexBuffer[vertexBuffers.Length];
		Array.Copy( vertexBuffers, this.vertexBuffers, vertexBuffers.Length );
	}

	/// <summary>
	/// The amount of uploaded vertices stored by vertice buffers
	/// </summary>
	public int VertexCount => vertexBuffers[0].Count;

	/// <summary>
	/// Draws the elements specified by the <see cref="IElementBuffer"/>
	/// </summary>
	/// <param name="count">The amount of indices to draw. For example, this means that if the element buffer is a triangle buffer, this number needs to be a multiple of 3</param>
	/// <param name="offset">The amount of indices to offset by</param>
	public void Draw ( int count, int offset = 0 )
		=> ElementBuffer!.Draw( count, offset );

	/// <summary>
	/// Draws all elements specified by the <see cref="IElementBuffer"/>
	/// </summary>
	public void Draw ()
		=> ElementBuffer!.Draw();

	/// <summary>
	/// Draws vertices using a given primitive without an <see cref="IElementBuffer"/>
	/// </summary>
	/// <param name="count">The amount of vertices to draw. For example, this means that if the primitive is a triangle, this number needs to be a multiple of 3</param>
	/// <param name="offset">The amount of vertices to offset by</param>
	public void Draw ( PrimitiveType primitive, int count, int offset = 0 )
		=> GL.DrawArrays( primitive, offset, count );

	/// <summary>
	/// Draws all vertices using a given primitive without an <see cref="IElementBuffer"/>
	/// </summary>
	public void Draw ( PrimitiveType primitive )
		=> Draw( primitive, VertexCount, 0 );

	/// <summary>
	/// Combines the uploads of <see cref="CreateVertexUpload(BufferUsageHint)"/> and <see cref="CreateElementBufferUpload(BufferUsageHint)"/>
	/// into a single operation. You should use this to prevent only one of the buffers being updated, then drawn, then the other updated the next frame
	/// </summary>
	public IUpload CreateFullUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw, BufferUsageHint? elementBufferUsage = null ) {
		var arr = MemoryPool<IUpload>.Shared.Rent( ( ElementBuffer is null ? 0 : 1 ) + vertexBuffers.Length );
		for ( int i = 0; i < vertexBuffers.Length; i++ ) {
			arr[i] = vertexBuffers[i].CreateUpload( usage );
		}
		if ( ElementBuffer != null )
			arr[arr.Length - 1] = ElementBuffer.CreateUpload( elementBufferUsage ?? usage );

		return new CombinedUpload( arr );
	}

	/// <summary>
	/// Combines the uploads of <see cref="CreateUnsafeVertexUpload(BufferUsageHint)(BufferUsageHint)"/> and <see cref="CreateUnsafeElementBufferUpload(BufferUsageHint)(BufferUsageHint)"/>
	/// into a single operation. You should use this to prevent only one of the buffers being updated, then drawn, then the other updated the next frame
	/// </summary>
	public IUpload CreateFullUnsafeUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw, BufferUsageHint? elementBufferUsage = null ) {
		var arr = MemoryPool<IUpload>.Shared.Rent( ( ElementBuffer is null ? 0 : 1 ) + vertexBuffers.Length );
		for ( int i = 0; i < vertexBuffers.Length; i++ ) {
			arr[i] = vertexBuffers[i].CreateUnsafeUpload( usage );
		}
		if ( ElementBuffer != null )
			arr[arr.Length - 1] = ElementBuffer.CreateUnsafeUpload( elementBufferUsage ?? usage );

		return new CombinedUpload( arr );
	}

	/// <summary>
	/// Uploads the data to the vertex buffers. The created upload will *copy*
	/// the current data and send it to the draw thread to make sure it is not modified while it is being uploaded.
	/// If you are *absolutely sure* the data will not be modified in that period, you can use <see cref="CreateUnsafeVertexUpload(BufferUsageHint)"/>
	/// to avoid copying data
	/// </summary>
	/// <remarks>
	/// You should use this only if the amount of vertices hasn't changed, but they have been modified, 
	/// or there is no element buffer.
	/// Otherwise you should use <see cref="CreateFullUpload(BufferUsageHint, BufferUsageHint?)"/>
	/// </remarks>
	public IUpload CreateVertexUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		var arr = MemoryPool<IUpload>.Shared.Rent( vertexBuffers.Length );
		for ( int i = 0; i < vertexBuffers.Length; i++ ) {
			arr[i] = vertexBuffers[i].CreateUpload( usage );
		}

		return new CombinedUpload( arr );
	}

	/// <summary>
	/// Uploads the data to the vertex buffers. The created upload will *not copy*
	/// the current data, therefore you need to make sure the data will *not be modified* while it is being uploaded
	/// </summary>
	/// <remarks>
	/// You should use this only if the amount of vertices hasn't changed, but they have been modified, 
	/// or there is no element buffer.
	/// Otherwise you should use <see cref="CreateFullUnsafeUpload(BufferUsageHint, BufferUsageHint?)"/>
	/// </remarks>
	public IUpload CreateUnsafeVertexUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		var arr = MemoryPool<IUpload>.Shared.Rent( vertexBuffers.Length );
		for ( int i = 0; i < vertexBuffers.Length; i++ ) {
			arr[i] = vertexBuffers[i].CreateUnsafeUpload( usage );
		}

		return new CombinedUpload( arr );
	}

	/// <inheritdoc cref="IElementBuffer.CreateUpload(BufferUsageHint)"/>
	/// <remarks>
	/// If you are updating both the vertice buffers and the element buffer, you should use
	/// <see cref="CreateFullUpload(BufferUsageHint, BufferUsageHint?)"/>
	/// </remarks>
	public IUpload CreateElementBufferUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw )
		=> ElementBuffer!.CreateUpload( usage );

	/// <inheritdoc cref="IElementBuffer.CreateUnsafeUpload(BufferUsageHint)"/>
	/// <remarks>
	/// If you are updating both the vertice buffers and the element buffer, you should use
	/// <see cref="CreateFullUnsafeUpload(BufferUsageHint, BufferUsageHint?)"/>
	/// </remarks>
	public IUpload CreateUnsafeElementBufferUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw )
		=> ElementBuffer!.CreateUnsafeUpload( usage );

	public void Dispose () {
		ElementBuffer?.Dispose();
		foreach ( var i in vertexBuffers )
			i.Dispose();
	}
}