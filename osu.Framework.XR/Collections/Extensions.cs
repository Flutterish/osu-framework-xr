using System.Runtime.InteropServices;

namespace osu.Framework.XR.Collections;

public static class Extensions {
	/// <inheritdoc cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>
	public static Span<T> AsSpan<T> ( this List<T> list )
		=> CollectionsMarshal.AsSpan( list );

	public static T At<T> ( this IList<T> self, int index, T @default = default! )
		=> self.Count > index ? self[index] : @default;
}
