using osu.Framework.Platform;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace osu.Framework.XR.Parsing.Materials {
	public class MTLFileReference {
		public MTLFileReference ( string path ) {
			Path = path;
		}

		public readonly string Path;
		[MemberNotNullWhen( true, nameof( Source ) )]
		public bool IsLoaded { get; private set; }
		[MemberNotNull(nameof(Source))]
		public void Load ( Storage baseDirectory ) {
			if ( IsLoaded ) return;

			using var stream = new StreamReader( baseDirectory.GetStream( Path ) );
			Source = MTLFile.FromText( stream.ReadToEnd() );
			IsLoaded = true;
		}
		[MemberNotNull( nameof( Source ) )]
		public void Load ( string baseDirectory ) {
			if ( IsLoaded ) return;

			using var stream = new StreamReader( System.IO.Path.Combine( System.IO.Path.GetFullPath( baseDirectory ), Path ) );
			Source = MTLFile.FromText( stream.ReadToEnd(), baseDirectory );
			IsLoaded = true;
		}
		public MTLFile? Source;
	}
}
