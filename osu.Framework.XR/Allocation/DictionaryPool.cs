using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;

namespace osu.Framework.XR.Allocation {
	public class DictionaryPool<K,V> where K : notnull {
		private DictionaryPool () {
			pool = ObjectPool.Create( new PoolPolicy( this ) );
		}

		private ObjectPool<PooledDictionary<K,V>> pool;

		public static PooledDictionary<K, V> Shared { get; } = new();

		public PooledDictionary<K, V> Rent ()
			=> pool.Get();

		public void Return ( PooledDictionary<K, V> list )
			=> pool.Return( list );

		private class PoolPolicy : IPooledObjectPolicy<PooledDictionary<K,V>> {
			private DictionaryPool<K,V> parent;
			public PoolPolicy ( DictionaryPool<K,V> parent ) {
				this.parent = parent;
			}

			public PooledDictionary<K,V> Create ()
				=> new( parent );

			public bool Return ( PooledDictionary<K, V> obj ) {
				obj.Clear();
				return true;
			}
		}
	}

	public class PooledDictionary<K,V> : Dictionary<K,V>, IDisposable where K : notnull {
		private DictionaryPool<K,V> pool;
		public PooledDictionary () {
			throw new InvalidOperationException( "This constructor should never be called." );
		}

		public PooledDictionary ( DictionaryPool<K, V> pool ) {
			this.pool = pool;
		}

		public void Dispose () {
			pool.Return( this );
		}
	}
}
