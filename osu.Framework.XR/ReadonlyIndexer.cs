using System;

namespace osu.Framework.XR {
	public class ReadonlyIndexer<Tin, Tout> {
		protected Func<Tin, Tout> Getter;

		public ReadonlyIndexer ( Func<Tin, Tout> getter ) {
			this.Getter = getter;
		}

		public virtual Tout this[ Tin index ]
			=> Getter( index );
	}
}
