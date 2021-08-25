using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;

namespace osu.Framework.XR.Allocation {
	public class ListPool<T> {
		private ListPool () {
			pool = ObjectPool.Create( new PoolPolicy( this ) );
		}

		private ObjectPool<PooledList<T>> pool;

		public static ListPool<T> Shared { get; } = new();

		public PooledList<T> Rent ()
			=> pool.Get();

		public void Return ( PooledList<T> list )
			=> pool.Return( list );

		private class PoolPolicy : IPooledObjectPolicy<PooledList<T>> {
			private ListPool<T> parent;
			public PoolPolicy ( ListPool<T> parent ) {
				this.parent = parent;
			}

			public PooledList<T> Create ()
				=> new( parent );

			public bool Return ( PooledList<T> obj ) {
				obj.Clear();
				return true;
			}
		}
	}

	public class PooledList<T> : List<T>, IDisposable {
		private ListPool<T> pool;
		public PooledList () {
			throw new InvalidOperationException( "This constructor should never be called." );
		}

		public PooledList ( ListPool<T> pool ) {
			this.pool = pool;
		}

		public void Dispose () {
			pool.Return( this );
		}
	}
}
