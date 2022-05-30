using System.Buffers;

namespace osu.Framework.XR.Allocation {
	/// <summary>
	/// Wrapper for <see cref="ArrayPool{T}"/> which returns <see cref="RentedArray{T}"/>.
	/// </summary>
	/// <remarks>
	/// <code>
	/// using (MemoryPool&lt;T&gt;.Shared.Rent(100, out var span)) { ... }
	/// </code>
	/// </remarks>
	public class MemoryPool<T> {
        private readonly ArrayPool<T> backing;

        public MemoryPool ( ArrayPool<T> backing ) {
            this.backing = backing;
        }

        public static MemoryPool<T> Shared { get; } = new( ArrayPool<T>.Shared );

        public RentedArray<T> Rent ( ICollection<T> value ) {
            var arr = backing.Rent( value.Count );
            value.CopyTo( arr, 0 );
            return new RentedArray<T>( backing, arr, value.Count );
        }

        public RentedArray<T> Rent ( ICollection<T> value, out Span<T> span ) {
            var arr = backing.Rent( value.Count );
            value.CopyTo( arr, 0 );
            span = arr;
            return new RentedArray<T>( backing, arr, value.Count );
        }

        public RentedArray<T> Rent ( int size )
            => new( backing, backing.Rent( size ), size );

        public RentedArray<T> Rent ( int size, out Span<T> span ) {
            var arr = new RentedArray<T>( backing, backing.Rent( size ), size );
            span = arr;
            return arr;
        }
    }

    public readonly struct RentedArray<T> : IDisposable {
        private readonly ArrayPool<T> backing;
        private readonly T[] rented;
        public readonly int Length;

        public RentedArray ( ArrayPool<T> backing, T[] rented, int length ) {
            this.backing = backing;
            this.rented = rented;
            Length = length;
        }

        public Span<T> AsSpan () => rented.AsSpan( 0, Length );

        public Span<T>.Enumerator GetEnumerator () => rented.AsSpan( 0, Length ).GetEnumerator();

        public ref T this[int i] => ref rented[i];
        public ref T this[Index i] => ref rented[i];

        public static implicit operator Span<T> ( RentedArray<T> self )
            => self.AsSpan();

        public void Dispose () {
            backing?.Return( rented );
        }
    }
}
