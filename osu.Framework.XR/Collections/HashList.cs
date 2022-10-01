using System.Collections;
using System.Runtime.InteropServices;

namespace osu.Framework.XR.Collections;

/// <summary>
/// A combination of <see cref="List{T}"/>'s iteration speed and <see cref="HashSet{T}"/>'s insert/remove speed.
/// The order or elements within this collection is not guaranteed to be the same as of a list.
/// For the fastest iteration speed, use <see cref="AsSpan"/>
/// </summary>
public class HashList<T> : IEnumerable<T> where T : notnull {
	List<T> data = new();
	Dictionary<T, int> indices = new();

	public void Add ( T item ) {
		indices.Add( item, data.Count );
		data.Add( item );
	}

	public void Remove ( T item ) {
		if ( indices.Remove( item, out var index ) ) {
			var lastIndex = data.Count - 1;
			if ( index == lastIndex ) {
				data.RemoveAt( lastIndex );
			}
			else {
				item = data[lastIndex];
				data.RemoveAt( lastIndex );

				data[index] = item;
				indices[item] = index;
			}
		}
	}

	public void Clear () {
		data.Clear();
		indices.Clear();
	}

	public int IndexOf ( T item )
		=> indices[item];

	public int Count => data.Count;
	public T this[int index] {
		get => data[index];
		set {
			var old = data[index];
			indices.Remove( old );
			indices.Add( value, index );
			data[index] = value;
		}
	}

	/// <summary>
	/// Gets a <see cref="ReadOnlySpan{T}"/> view over the data in a list. Items should not be added
	/// or removed from the <see cref="HashList{T}"/> while the <see cref="ReadOnlySpan{T}"/> is in use
	/// </summary>
	public ReadOnlySpan<T> AsSpan () => CollectionsMarshal.AsSpan( data );
	public Span<T>.Enumerator GetEnumerator () => CollectionsMarshal.AsSpan( data ).GetEnumerator();

	IEnumerator<T> IEnumerable<T>.GetEnumerator () {
		return ( (IEnumerable<T>)data ).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator () {
		return ( (IEnumerable)data ).GetEnumerator();
	}
}
