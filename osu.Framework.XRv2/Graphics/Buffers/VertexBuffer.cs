using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics.Vertices;
using System.Runtime.InteropServices;

namespace osu.Framework.XR.Graphics.Buffers;

/// <summary>
/// A vertex buffer is a GPU array of geometry data such as position, uv coordinates, normals,
/// or any other custom data that is different per drawn vertex. A given property of a vertex 
/// such as position, a normal, etc. is called an attribute, or an attrib.
/// Multiple vertice buffers can be used to draw a single object (as long as they are the same length) 
/// and they can be shared across different meshes
/// </summary>
public interface IVertexBuffer {
	/// <inheritdoc cref="IVertex{Tself}.Stride"/>
	int Stride { get; }
	/// <inheritdoc cref="IVertex{Tself}.Link(Shader, int[])"/>
	void Link ( Shader shader, int[] attribs );

	/// <summary>
	/// Uploads the data to the vertex buffer. The created upload will *copy*
	/// the current data and send it to the draw thread to make sure it is not modified while it is being uploaded.
	/// If you are *absolutely sure* the data will not be modified in that peroid, you can use <see cref="CreateUnsafeUpload(BufferUsageHint)"/>
	/// to avoid copying data
	/// </summary>
	IUpload CreateUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw );

	/// <summary>
	/// Uploads the data to the vertex buffer. The created upload will *not copy*
	/// the current data, therefore you need to make sure the data will *not be modified* while it is being uploaded
	/// </summary>
	IUpload CreateUnsafeUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw );

	/// <summary>
	/// The amount of uploaded vertices stored by this buffer
	/// </summary>
	int Count { get; }
}

/// <inheritdoc cref="IVertexBuffer"/>
public class VertexBuffer<Tvertex> : IVertexBuffer where Tvertex : struct, IVertex<Tvertex> {
	public readonly List<Tvertex> Data = new();
	public GlHandle Handle { get; private set; }

	public int Count { get; private set; }

	public int Stride => default(Tvertex).Stride;

	public void Link ( Shader shader, int[] attribs )
		=> default(Tvertex).Link( shader, attribs );

	public IUpload CreateUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		return new Upload( this, usage );
	}

	public IUpload CreateUnsafeUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		return new UnsafeUpload( this, usage );
	}

	class Upload : IUpload {
		RentedArray<Tvertex> data;
		BufferUsageHint usage;
		VertexBuffer<Tvertex> source;

		public Upload ( VertexBuffer<Tvertex> source, BufferUsageHint usage ) {
			data = MemoryPool<Tvertex>.Shared.Rent( source.Data );
			this.usage = usage;
			this.source = source;
		}

		void IUpload.Upload () {
			if ( source.Handle == 0 )
				source.Handle = GL.GenBuffer();

			GL.BindBuffer( BufferTarget.ArrayBuffer, source.Handle );
			default(Tvertex).Upload( data, usage );
			source.Count = data.Length;
			data.Dispose();
		}
	}

	class UnsafeUpload : IUpload {
		VertexBuffer<Tvertex> source;
		BufferUsageHint usage;

		public UnsafeUpload ( VertexBuffer<Tvertex> source, BufferUsageHint usage ) {
			this.source = source;
			this.usage = usage;
		}

		void IUpload.Upload () {
			if ( source.Handle == 0 )
				source.Handle = GL.GenBuffer();

			GL.BindBuffer( BufferTarget.ArrayBuffer, source.Handle );
			default(Tvertex).Upload( CollectionsMarshal.AsSpan( source.Data ), usage );
			source.Count = source.Data.Count;
		}
	}
}