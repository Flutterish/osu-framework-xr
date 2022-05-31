using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.XR.Allocation;
using System.Runtime.InteropServices;

namespace osu.Framework.XR.Graphics.Buffers;

public interface IElementBuffer {
	/// <summary>
	/// Uploads the data to the element buffer. The created upload will *copy*
	/// the current data and send it to the draw thread to make sure it is not modified while it is being uploaded.
	/// If you are *absolutely sure* the data will not be modified in that peroid, you can use <see cref="CreateUnsafeUpload(BufferUsageHint)"/>
	/// to avoid copying data
	/// </summary>
	IUpload CreateUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw );

	/// <summary>
	/// Uploads the data to the element buffer. The created upload will *not copy*
	/// the current data, therefore you need to make sure the data will *not be modified* while it is being uploaded
	/// </summary>
	IUpload CreateUnsafeUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw );

	/// <summary>
	/// The amount of uploaded indices stored by this buffer
	/// </summary>
	int Count { get; }

	/// <summary>
	/// Draws the elements specified by the indices of this buffer
	/// </summary>
	/// <param name="count">The amount of indices to draw. For example, this means that if this is a triangle buffer, this number needs to be a multiple of 3</param>
	/// <param name="offset">The amount of indices to offset by</param>
	void Draw ( int count, int offset = 0 );

	/// <summary>
	/// Draws all elements specified by the indices of this buffer
	/// </summary>
	public void Draw ()
		=> Draw( Count, 0 );
}

public class ElementBuffer<Tindex> : IElementBuffer where Tindex : unmanaged {
	public readonly List<Tindex> Indices = new();
	public readonly PrimitiveType PrimitiveType;
	public static readonly DrawElementsType ElementType;
	public static readonly int Stride;
	public GlHandle Handle { get; private set; }

	public int Count { get; private set; }

	static ElementBuffer () {
		Stride = Marshal.SizeOf<Tindex>();
		ElementType = default( Tindex ) switch {
			byte => DrawElementsType.UnsignedByte,
			ushort => DrawElementsType.UnsignedShort,
			uint => DrawElementsType.UnsignedInt,
			_ => throw new NotSupportedException( $"An element buffer might only contain Byte, UInt16 or UInt32 indices, but {typeof(Tindex).ReadableName()} was specified" )
		};
	}

	public ElementBuffer ( PrimitiveType primitive = PrimitiveType.Triangles ) {
		PrimitiveType = primitive;
	}

	public IUpload CreateUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		return new Upload( this, usage );
	}

	public IUpload CreateUnsafeUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		return new UnsafeUpload( this, usage );
	}

	public void Draw ( int count, int offset = 0 ) {
		GL.DrawElements( PrimitiveType, count, ElementType, offset * Stride );
	}

	class Upload : IUpload {
		RentedArray<Tindex> data;
		BufferUsageHint usage;
		ElementBuffer<Tindex> source;

		public Upload ( ElementBuffer<Tindex> source, BufferUsageHint usage ) {
			data = MemoryPool<Tindex>.Shared.Rent( source.Indices );
			this.source = source;
			this.usage = usage;
		}

		void IUpload.Upload () {
			if ( source.Handle == 0 )
				source.Handle = GL.GenBuffer();

			GL.BindBuffer( BufferTarget.ElementArrayBuffer, source.Handle );
			GL.BufferData( BufferTarget.ElementArrayBuffer, data.Length * Stride, ref MemoryMarshal.GetReference( data.AsSpan() ), usage );
			source.Count = data.Length;
			data.Dispose();
		}
	}

	class UnsafeUpload : IUpload {
		ElementBuffer<Tindex> source;
		BufferUsageHint usage;

		public UnsafeUpload ( ElementBuffer<Tindex> source, BufferUsageHint usage ) {
			this.source = source;
			this.usage = usage;
		}

		void IUpload.Upload () {
			if ( source.Handle == 0 )
				source.Handle = GL.GenBuffer();

			GL.BindBuffer( BufferTarget.ElementArrayBuffer, source.Handle );
			GL.BufferData( BufferTarget.ElementArrayBuffer, source.Indices.Count * Stride, ref MemoryMarshal.GetReference( CollectionsMarshal.AsSpan( source.Indices ) ), usage );
			source.Count = source.Indices.Count;
		}
	}
}