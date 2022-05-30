using System.Runtime.InteropServices;

namespace osu.Framework.XR.Graphics.Vertices;

public interface IVertex<Tself> where Tself : struct, IVertex<Tself> {
	/// <summary>
	/// The size of a single vertex. This should be equal to <see cref="Marshal.SizeOf{Tself}"/>
	/// </summary>
	static abstract int Stride { get; }
	/// <summary>
	/// Uploads the data to the currently bound array buffer
	/// </summary>
	static void Upload ( Span<Tself> data, BufferUsageHint usage = BufferUsageHint.StaticDraw ) {
		GL.BufferData( BufferTarget.ArrayBuffer, data.Length * Tself.Stride, ref MemoryMarshal.GetReference( data ), usage );
	}
	/// <summary>
	/// Tells the currently bound vertex array how the data of the currenly bound array buffer should be interpreted
	/// </summary>
	/// <param name="attribs">An array of attrib positions the array buffer uses</param>
	static abstract void Link ( Shader shader, int[] attribs );
}