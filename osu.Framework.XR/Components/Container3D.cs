using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Components {
	public class Container3D : CompositeDrawable3D {
		new public Drawable3D Child {
			get => children.Single();
			set {
				foreach ( var i in children.ToArray() ) i.Parent = null;
				value.Parent = this;
			}
		}
		new public IReadOnlyList<Drawable3D> Children {
			get => children.AsReadOnly();
			set {
				foreach ( var i in children.ToArray() ) i.Parent = null;
				foreach ( var i in value ) i.Parent = this;
			}
		}
		new public void Add ( Drawable3D child ) {
			child.Parent = this;
		}
		new public void Remove ( Drawable3D child ) {
			if ( child.parent != this ) throw new InvalidOperationException( "Tried to remove child which does not belong to this parent." );
			child.Parent = null;
		}
	}
}
