using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Projection {
	/// <summary>
	/// Marks this <see cref="Components.Drawable3D"/> to be rendered after everything else.
	/// This interface is a temporary fix before tris-sorting of transparent objects.
	/// </summary>
	public interface IRenderedLast { }
}
