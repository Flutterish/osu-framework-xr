using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Allocation;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace osu.Framework.XR.Graphics.Buffers;

/// <summary>
/// A GPU array of indices which point into vertice buffers in order to not duplicate vertex data. 
/// Elements such as triangles or lines are created using a method defined by <see cref="PrimitiveType"/>. 
/// </summary>
/// <remarks>
/// A given mesh can only use one element buffer, but that buffer can be shared across different meshes. 
/// It is also possible to create several "partial" meshes, each using a different element buffer, 
/// but the same vertice buffers and possibly a different material.
/// Note that drawing with an element buffer requires a linked attribute array to be bound
/// </remarks>
public interface IElementBuffer : IDisposable {
	/// <summary>
	/// Uploads the data to the element buffer. The created upload will *copy*
	/// the current data and send it to the draw thread to make sure it is not modified while it is being uploaded.
	/// If you are *absolutely sure* the data will not be modified in that period, you can use <see cref="CreateUnsafeUpload(BufferUsageHint)"/>
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
	/// Binds the <see cref="IElementBuffer"/> to the currently bound <see cref="IAttributeArray"/>
	/// </summary>
	void Bind ();

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

/// <inheritdoc cref="IElementBuffer"/>
public class ElementBuffer<Tindex> : IElementBuffer where Tindex : unmanaged {
	public readonly List<Tindex> Indices = new();
	public readonly PrimitiveType PrimitiveType;
	public static readonly DrawElementsType ElementType;
	public static readonly int Stride;
	public GlHandle Handle { get; private set; }

	public int Count { get; private set; }
	bool isDisposed;

	static ElementBuffer () {
		Stride = Marshal.SizeOf<Tindex>();
		ElementType = default( Tindex ) switch {
			byte => DrawElementsType.UnsignedByte,
			ushort => DrawElementsType.UnsignedShort,
			uint => DrawElementsType.UnsignedInt,
			_ => throw new NotSupportedException( $"An element buffer might only contain Byte, UInt16 or UInt32 indices, but {typeof( Tindex ).ReadableName()} was specified" )
		};
	}

	public ElementBuffer ( PrimitiveType primitive = PrimitiveType.Triangles ) {
		PrimitiveType = primitive;
	}

	public void Bind () {
		throwIfDisposed();

		if ( Handle == 0 )
			Handle = GL.GenBuffer();

		GL.BindBuffer( BufferTarget.ElementArrayBuffer, Handle );
	}

	ulong uploadID;
	public IUpload CreateUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		return new Upload( this, usage );
	}

	public IUpload CreateUnsafeUpload ( BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		return new UnsafeUpload( this, usage );
	}

	public void Draw ( int count, int offset = 0 ) {
		GL.DrawElements( PrimitiveType, count, ElementType, offset * Stride );
	}

	public void Dispose () {
		isDisposed = true;
		DisposeScheduler.Enqueue( this, v => {
			GL.DeleteBuffer( Handle );
			v.Handle = 0;
		} );
		uploadID++;
		GC.SuppressFinalize( this );
	}

	~ElementBuffer () {
		throwIfNotDisposed();
	}

	[Conditional( "DEBUG" )]
	void throwIfDisposed () {
		if ( isDisposed )
			throw new InvalidOperationException( $"Used an {nameof( AttributeArray )} after disposal" );
	}

	[Conditional( "DEBUG" )]
	void throwIfNotDisposed () {
		if ( !isDisposed )
			throw new InvalidOperationException( $"An {nameof( AttributeArray )} has not been disposed correctly" );
	}

	class Upload : IUpload {
		RentedArray<Tindex> data;
		BufferUsageHint usage;
		ElementBuffer<Tindex> source;
		ulong id;

		public Upload ( ElementBuffer<Tindex> source, BufferUsageHint usage ) {
			id = ++source.uploadID;
			data = MemoryPool<Tindex>.Shared.Rent( source.Indices );
			this.source = source;
			this.usage = usage;
		}

		void IUpload.Upload ( IRenderer renderer ) {
			if ( id != source.uploadID ) {
				data.Dispose();
				return;
			}

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

		void IUpload.Upload ( IRenderer renderer ) {
			if ( source.Handle == 0 )
				source.Handle = GL.GenBuffer();

			GL.BindBuffer( BufferTarget.ElementArrayBuffer, source.Handle );
			GL.BufferData( BufferTarget.ElementArrayBuffer, source.Indices.Count * Stride, ref MemoryMarshal.GetReference( CollectionsMarshal.AsSpan( source.Indices ) ), usage );
			source.Count = source.Indices.Count;
		}
	}
}