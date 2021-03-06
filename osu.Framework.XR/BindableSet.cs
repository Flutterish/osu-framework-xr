using System;
using System.Collections;
using System.Collections.Generic;

namespace osu.Framework.XR {
	public class BindableSet<T> : IEnumerable<T> {
		private HashSet<T> set = new();

		public event Action<T>? ItemAdded;
		public event Action<T>? ItemRemoved;
		public event Action? CollectionChanged;

		public void BindCollectionChanged ( Action action, bool runOnceImmediately = false ) {
			CollectionChanged += action;
			if ( runOnceImmediately ) action();
		}

		public void BindItemAdded ( Action<T> action, bool runOnceImmediately = false ) {
			ItemAdded += action;
			if ( runOnceImmediately ) {
				foreach ( var i in set ) {
					action( i );
				}
			}
		}

		public void Add ( T value ) {
			if ( set.Contains( value ) ) return;

			set.Add( value );
			ItemAdded?.Invoke( value );
			CollectionChanged?.Invoke();
		}

		public void Remove ( T value ) {
			if ( !set.Contains( value ) ) return;

			set.Remove( value );
			ItemRemoved?.Invoke( value );
			CollectionChanged?.Invoke();
		}

		public IEnumerator<T> GetEnumerator () {
			return ( (IEnumerable<T>)set ).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return ( (IEnumerable)set ).GetEnumerator();
		}
	}
}
