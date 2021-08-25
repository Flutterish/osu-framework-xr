using osu.Framework.XR.Graphics;
using System.Collections.Generic;

namespace osu.Framework.XR.Parsing {
	public class ImportedModelGroup {
		public ImportedModelGroup ( string name ) {
			Name = name;
		}

		public readonly string Name;

		public readonly List<ImportedModelGroup> SubGroups = new();
		public readonly List<ImportedModel> Models = new();
	}

	public class ImportedModel {
		public ImportedModel ( string name ) {
			Name = name;
		}

		public readonly string Name;
		public readonly List<(Mesh mesh, ImportedMaterial material)> Elements = new();
	}

	public class ImportedMaterial {
		public static readonly ImportedMaterial Default = new();
	}
}
