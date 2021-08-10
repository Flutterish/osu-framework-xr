using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Components {
	public class Container3D : CompositeDrawable3D {
		/// <summary>
		/// What <see cref="Container3D"/> the children actually get added to/removed from. 
		/// This is used if you want to separate content from internal children. 
		/// If the content is this, it adds to internal children.
		/// </summary>
		protected virtual Container3D Content => this;

		public Drawable3D Child {
			get => Content == this ? InternalChild : Content.Child;
			set {
				if ( Content == this ) InternalChild = value;
				else Content.Child = value;
			}
		}
		public IReadOnlyList<Drawable3D> Children {
			get => Content == this ? InternalChildren : Content.Children;
			set {
				if ( Content == this ) InternalChildren = value;
				else Content.Children = value;
			}
		}
		public void Add ( Drawable3D child ) {
			if ( Content == this ) AddInternal( child );
			else Content.Add( child );
		}
		public void Remove ( Drawable3D child ) {
			if ( Content == this ) RemoveInternal( child );
			else Content.Remove( child );
		}
		public void Clear ( bool disposeChildren = true ) {
			if ( Content == this ) ClearInternal( disposeChildren );
			else Content.Clear( disposeChildren );
		}
	}
}
