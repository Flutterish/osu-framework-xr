using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Components {
	public class XrGroup : CompositeXrObject {
		new public XrObject Child {
			get => children.Single();
			set {
				foreach ( var i in children.ToArray() ) i.Parent = null;
				value.Parent = this;
			}
		}
		new public IReadOnlyList<XrObject> Children {
			get => children.AsReadOnly();
			set {
				foreach ( var i in children.ToArray() ) i.Parent = null;
				foreach ( var i in value ) i.Parent = this;
			}
		}
		new public void Add ( XrObject child ) {
			child.Parent = this;
		}
		new public void Remove ( XrObject child ) {
			if ( child.parent != this ) throw new InvalidOperationException( "Tried to remove child which does not belong to this parent." );
			child.Parent = null;
		}
	}
}
