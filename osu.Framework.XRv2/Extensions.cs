using System.Runtime.InteropServices;

namespace osu.Framework.XR;

public static class Extensions {
	/// <inheritdoc cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>
	public static Span<T> AsSpan<T> ( this List<T> list )
		=> CollectionsMarshal.AsSpan( list );
}
