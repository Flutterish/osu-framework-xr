using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;

namespace osu.Framework.XR.Allocation {
	public class StackPool<T> {
		public StackPool () {
			pool = new StackObjectPool<PooledStack<T>>( new PoolPolicy( this ) );
		}

		private ObjectPool<PooledStack<T>> pool;

		[ThreadStatic]
		private static StackPool<T>? shared;

		public static StackPool<T> Shared => shared ??= new();

		public PooledStack<T> Rent ()
			=> pool.Get();

		public PooledStack<T> Rent ( T content ) {
			var list = pool.Get();
			list.Push( content );
			return list;
		}

		public void Return ( PooledStack<T> list )
			=> pool.Return( list );

		private class PoolPolicy : IPooledObjectPolicy<PooledStack<T>> {
			private StackPool<T> parent;
			public PoolPolicy ( StackPool<T> parent ) {
				this.parent = parent;
			}

			public PooledStack<T> Create ()
				=> new( parent );

			public bool Return ( PooledStack<T> obj ) {
				obj.Clear();
				return true;
			}
		}
	}

	public class PooledStack<T> : Stack<T>, IDisposable {
		private StackPool<T> pool;
		public PooledStack () {
			throw new InvalidOperationException( "This constructor should never be called." );
		}

		public PooledStack ( StackPool<T> pool ) {
			this.pool = pool;
		}

		public void Dispose () {
			pool.Return( this );
		}
	}
}
