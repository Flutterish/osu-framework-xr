using osu.Framework.XR.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing {
	public class ModelGroup {
		public readonly List<ModelGroup> SubGroups = new();
		public readonly List<Mesh> Models = new();
	}
}
