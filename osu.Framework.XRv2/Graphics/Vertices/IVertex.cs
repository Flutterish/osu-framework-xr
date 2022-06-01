using System.Runtime.InteropServices;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Shaders;

namespace osu.Framework.XR.Graphics.Vertices;

public interface IVertex {
	/// <summary>
	/// The size of a single vertex. This should be equal to <see cref="Marshal.SizeOf{Tself}"/>
	/// </summary>
	int Stride { get; }

	/// <summary>
	/// Tells the currently bound <see cref="IAttributeArray"/> how the data of the <see cref="IVertexBuffer"/> should be interpreted
	/// </summary>
	/// <param name="attribs">An array of attrib positions the array buffer uses</param>
	void Link ( Shader shader, int[] attribs );
}

public interface IVertex<Tself> : IVertex where Tself : struct, IVertex<Tself> {
	/// <summary>
	/// Uploads the data to the currently bound <see cref="IVertexBuffer"/>
	/// </summary>
	void Upload ( Span<Tself> data, BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		GL.BufferData( BufferTarget.ArrayBuffer, data.Length * Stride, ref MemoryMarshal.GetReference( data ), usage );
	}
}