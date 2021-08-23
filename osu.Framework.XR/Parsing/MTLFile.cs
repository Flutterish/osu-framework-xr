using osu.Framework.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing {
	public class MTLFile {

	}

	public class MTLFileReference {
		public MTLFileReference ( string path ) {
			Path = path;
		}

		public readonly string Path;
		[MemberNotNullWhen(true, nameof(Source))]
		public bool IsLoaded { get; private set; }
		public void Load ( Storage baseDirectory ) {
			throw new NotImplementedException( "MTL file loading is not implemented yet." );
		}
		public MTLFile? Source;
	}
}
