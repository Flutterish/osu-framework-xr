using osu.Framework.XR.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing {
	/// <summary>
	/// A file or data structure which can be parsed into a collection of models along with their shaders, textures, materials, etc.
	/// </summary>
	public interface IModelFile {
		/// <summary>
		/// Creates a collection of models.
		/// </summary>
		ModelGroup CreateModelGroup ();
	}
}
