using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics.Vertices;
using System.Runtime.InteropServices;

namespace osu.Framework.XR.Graphics.Buffers;

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
}

public class VertexBuffer<Tvertex> : IVertexBuffer where Tvertex : struct, IVertex<Tvertex> {
	public readonly List<Tvertex> Data = new();

	public int Stride => Tvertex.Stride;

	public void Link ( Shader shader, int[] attribs )
		=> Tvertex.Link( shader, attribs );

	public IUpload CreateUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		return new Upload( MemoryPool<Tvertex>.Shared.Rent( Data ), usage );
	}

	public IUpload CreateUnsafeUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		return new UnsafeUpload( Data, usage );
	}

	class Upload : IUpload {
		RentedArray<Tvertex> data;
		BufferUsageHint usage;

		public Upload ( RentedArray<Tvertex> data, BufferUsageHint usage ) {
			this.data = data;
			this.usage = usage;
		}

		void IUpload.Upload () {
			IVertex<Tvertex>.Upload( data, usage );
		}

		public void Dispose () {
			data.Dispose();
		}
	}

	class UnsafeUpload : IUpload {
		List<Tvertex> data;
		BufferUsageHint usage;

		public UnsafeUpload ( List<Tvertex> data, BufferUsageHint usage ) {
			this.data = data;
			this.usage = usage;
		}

		void IUpload.Upload () {
			IVertex<Tvertex>.Upload( CollectionsMarshal.AsSpan( data ), usage );
		}

		public void Dispose () { }
	}
}