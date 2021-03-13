using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR {
	public class CachedReadonlyIndexer<Tin, Tout> : ReadonlyIndexer<Tin, Tout> {
		Dictionary<Tin, Tout> cache = new();

		public CachedReadonlyIndexer ( Func<Tin, Tout> getter ) : base( getter ) { }

		public override Tout this[ Tin index ] {
			get {
				if ( cache.TryGetValue( index, out var v ) ) return v;
				cache.Add( index, Getter( index ) );
				return cache[ index ];
			}
		}

		public void ClearCache () {
			cache.Clear();
		}
	}
}
