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

	void Draw ( int count, int offset = 0 );
}

public class ElementBuffer<Tindex> : IElementBuffer where Tindex : unmanaged {
	public readonly List<Tindex> Indices = new();
	public readonly PrimitiveType PrimitiveType;
	public static readonly DrawElementsType ElementType;
	public static readonly int Stride;

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
		return new Upload( MemoryPool<Tindex>.Shared.Rent( Indices ), usage );
	}

	public IUpload CreateUnsafeUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		return new UnsafeUpload( Indices, usage );
	}

	public void Draw ( int count, int offset = 0 ) {
		GL.DrawElements( PrimitiveType, count, ElementType, offset * Stride );
	}

	class Upload : IUpload {
		RentedArray<Tindex> data;
		BufferUsageHint usage;

		public Upload ( RentedArray<Tindex> data, BufferUsageHint usage ) {
			this.data = data;
			this.usage = usage;
		}

		void IUpload.Upload () {
			GL.BufferData( BufferTarget.ElementArrayBuffer, data.Length * Stride, ref MemoryMarshal.GetReference( data.AsSpan() ), usage );
			data.Dispose();
		}
	}

	class UnsafeUpload : IUpload {
		List<Tindex> data;
		BufferUsageHint usage;

		public UnsafeUpload ( List<Tindex> data, BufferUsageHint usage ) {
			this.data = data;
			this.usage = usage;
		}

		void IUpload.Upload () {
			GL.BufferData( BufferTarget.ElementArrayBuffer, data.Count * Stride, ref MemoryMarshal.GetReference( CollectionsMarshal.AsSpan( data ) ), usage );
		}
	}
}