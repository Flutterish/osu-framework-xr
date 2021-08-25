using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;

namespace osu.Framework.XR.Allocation {
	public class StackObjectPool<T> : ObjectPool<T> where T : class {
		IPooledObjectPolicy<T> policy;

		public StackObjectPool ( IPooledObjectPolicy<T> policy ) {
			this.policy = policy;
		}

		private Stack<T> available = new();
		private HashSet<T> rented = new();

		public override T Get () {
			if ( !available.TryPop( out var item ) ) {
				item = policy.Create();
			}

			rented.Add( item );
			return item;
		}

		public override void Return ( T obj ) {
			if ( !rented.Remove( obj ) ) {
				throw new InvalidOperationException( "Cannot return an object to pool that it wasnt rented from" );
			}

			if ( policy.Return( obj ) )
				available.Push( obj );
		}
	}
}
