using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.XR.Maths;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Components {
	public class CompositeDrawable3D : Drawable3D {
		internal List<Drawable3D> children = new();
		new public Drawable3D InternalChild {
			get => children.Single();
			protected set {
				foreach ( var i in children.ToArray() ) i.Parent = null;
				value.Parent = this;
			}
		}
		new public IReadOnlyList<Drawable3D> InternalChildren {
			get => children.AsReadOnly();
			protected set {
				foreach ( var i in children.ToArray() ) i.Parent = null;
				foreach ( var i in value ) i.Parent = this;
			}
		}
		protected void AddInternal ( Drawable3D child ) {
			child.Parent = this;
		}
		protected override void AddInternal ( Drawable drawable ) {
			if ( drawable is Drawable3D d3 ) AddInternal( d3 );
			else AddDrawable( drawable );
		}
		protected void RemoveInternal ( Drawable3D child ) {
			if ( child.parent != this ) throw new InvalidOperationException( "Tried to remove child which does not belong to this parent." );
			child.Parent = null;
		}
		new protected void ClearInternal ( bool disposeChildren = true ) {
			foreach ( var i in InternalChildren ) {
				RemoveInternal( i );
				if ( disposeChildren ) i.Destroy();
			}
		}

		// NOTE These events are used for dependency inversion and observing the hierarchy.
		// NOTE we possibly might want to remove the parent parameter as every interested subscriber can keep track of the current parent in case of removal and in case of addition just check the .Parent property
		// NOTE additionally, the "added to/removed from this one specifically" events should get a name change as anyone subscribing to a Container3D with custom content wants to subscribe to the content and not the internal children.
		public delegate void ChildChangedHandler ( Drawable3D parent, Drawable3D child );
		/// <summary>
		/// Occurs whenever a child is added to this <see cref="CompositeDrawable3D"/>
		/// </summary>
		public event ChildChangedHandler? ChildAdded;
		/// <summary>
		/// Occurs whenever a child is removed from this <see cref="CompositeDrawable3D"/>
		/// </summary>
		public event ChildChangedHandler? ChildRemoved;
		/// <summary>
		/// Occurs whenever a <see cref="Drawable3D"/> is added under this <see cref="CompositeDrawable3D"/>
		/// </summary>
		public event ChildChangedHandler? ChildAddedToHierarchy;
		/// <summary>
		/// Occurs whenever a <see cref="Drawable3D"/> is removed from under this <see cref="CompositeDrawable3D"/>
		/// </summary>
		public event ChildChangedHandler? ChildRemovedFromHierarchy;

		internal void onChildAdded ( Drawable3D child ) {
			ChildAdded?.Invoke( this, child );
			onChildAddedToHierarchy( this, child );
		}
		internal void onChildAddedToHierarchy ( Drawable3D parent, Drawable3D child ) {
			ChildAddedToHierarchy?.Invoke( parent, child );
			this.parent?.onChildAddedToHierarchy( parent, child );
		}
		internal void onChildRemoved ( Drawable3D child ) {
			ChildRemoved?.Invoke( this, child );
			onChildRemovedFromHierarchy( this, child );
		}
		internal void onChildRemovedFromHierarchy ( Drawable3D parent, Drawable3D child ) {
			ChildRemovedFromHierarchy?.Invoke( parent, child );
			this.parent?.onChildRemovedFromHierarchy( parent, child );
		}
		public void BindHierarchyChange ( ChildChangedHandler added, ChildChangedHandler removed, bool runOnAllChildrenImmediately = false ) {
			if ( removed is not null ) ChildRemovedFromHierarchy += removed;
			if ( added is not null ) {
				ChildAddedToHierarchy += added;
				if ( runOnAllChildrenImmediately ) {
					foreach ( var i in GetAllChildrenInHiererchy() ) {
						added( i.parent!, i );
					}
				}
			}
		}
		public void BindLocalHierarchyChange ( ChildChangedHandler added, ChildChangedHandler removed, bool runOnAllChildrenImmediately = false ) {
			if ( removed is not null ) ChildRemoved += removed;
			if ( added is not null ) {
				ChildAdded += added;
				if ( runOnAllChildrenImmediately ) {
					foreach ( var i in InternalChildren ) {
						added( this, i );
					}
				}
			}
		}

		private Vector3 childSize;
		/// <summary>
		/// The size nescessary to fit all children
		/// </summary>
		new public Vector3 ChildSize { get => childSize; private set => childSize = value; } // TODO this is not what ChildSize actually is in o!f. The current use is what BoundingBox does.
		public override Vector3 RequiredParentSizeToFit => ChildSize;
		public override Vector3 Size { 
			get => ChildSize; 
			set {
				if ( AutoSizeAxes != Axes3D.None ) throw new InvalidOperationException( $"Cannot modify size of an autosized {nameof(Drawable3D)}." );
				ChildSize = value;
			} 
		}
		new protected Axes3D AutoSizeAxes = Axes3D.All; // TODO invalidation mechanism for this
		public override Vector3 Centre => children.Any() ? children.Average( x => x.Position + x.Centre ) : Vector3.Zero;

		protected override void Update () {
			base.Update();
			if ( children.Any() ) {
				ChildSize = new Vector3(
					AutoSizeAxes.HasFlagFast( Axes3D.X ) ? children.Max( c => c.BypassAutoSizeAxes.HasFlagFast( Axes3D.X ) ? 0 : c.RequiredParentSizeToFit.X ) : Size.X,
					AutoSizeAxes.HasFlagFast( Axes3D.Y ) ? children.Max( c => c.BypassAutoSizeAxes.HasFlagFast( Axes3D.Y ) ? 0 : c.RequiredParentSizeToFit.Y ) : Size.Y,
					AutoSizeAxes.HasFlagFast( Axes3D.Z ) ? children.Max( c => c.BypassAutoSizeAxes.HasFlagFast( Axes3D.Z ) ? 0 : c.RequiredParentSizeToFit.Z ) : Size.Z
				);
			}
			else {
				if ( AutoSizeAxes.HasFlagFast( Axes3D.X ) ) childSize.X = 0;
				if ( AutoSizeAxes.HasFlagFast( Axes3D.Y ) ) childSize.Y = 0;
				if ( AutoSizeAxes.HasFlagFast( Axes3D.Z ) ) childSize.Z = 0;
			}
		}

		public override void Destroy () {
			foreach ( var i in children ) i.Destroy();
			base.Destroy();
		}
	}
}
