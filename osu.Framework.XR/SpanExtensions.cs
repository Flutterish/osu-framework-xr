namespace osu.Framework.XR;

public static class SpanExtensions {
	public static SpanSplit Split ( this ReadOnlySpan<char> span, char separator, Span<Range> buffer ) {
		int count = 0;
		int startIndex = 0;
		int index = 0;
		while ( count < buffer.Length ) {
			while ( index < span.Length ) {
				var c = span[index];
				if ( c == separator ) {
					buffer[count++] = startIndex..(index++);
					break;
				}

				index++;
			}

			if ( index == span.Length ) {
				if ( count < buffer.Length ) {
					buffer[count++] = startIndex..index;
				}
				break;
			}
			startIndex = index;
		}

		return new() { Span = span, Length = count, Splits = buffer };
	}
}

public ref struct SpanSplit {
	public ReadOnlySpan<char> Span;
	public int Length;
	public Span<Range> Splits;

	public ReadOnlySpan<char> this[int index]
		=> Span[Splits[index]];

	public ReadOnlySpan<char> Get ( int index )
		=> index >= 0 && index < Length ? Span[Splits[index]] : string.Empty.AsSpan();
}