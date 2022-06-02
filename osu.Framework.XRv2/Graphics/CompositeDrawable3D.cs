using osu.Framework.Graphics;
using System.ComponentModel;

namespace osu.Framework.XR.Graphics;

public class CompositeDrawable3D : Drawable3D {
	List<Drawable3D> children = new();
	new protected IReadOnlyList<Drawable3D> InternalChildren { get; }
	new protected IReadOnlyList<Drawable3D> AliveInternalChildren => InternalChildren;

	public CompositeDrawable3D () {
		InternalChildren = children.AsReadOnly();
	}

	[EditorBrowsable( EditorBrowsableState.Never )]
	protected sealed override void AddInternal ( Drawable drawable )
		=> throw new InvalidOperationException( "Cannot add a 2D drawable into a 3D container" );
	protected virtual void AddInternal ( Drawable3D child ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( $"Cannot add a {nameof(Drawable3D)} into multiple containers" );

		child.Parent = this;
		children.Add( child );
		base.AddInternal( child );

		ChildAdded?.Invoke( child, this );
		subtreeAdded( this, this, child );
	}

	static void subtreeAdded ( CompositeDrawable3D root, CompositeDrawable3D comp, Drawable3D child ) {
		var parent = root;
		while ( parent != null ) {
			parent.SubtreeChildAdded?.Invoke( child, comp );
			parent = parent.Parent;
		}

		if ( child is CompositeDrawable3D childComp ) {
			foreach ( var i in childComp.InternalChildren ) {
				subtreeAdded( root, childComp, i );
			}
		}
	}

	protected void AddRangeInternal ( IEnumerable<Drawable3D> children ) {
		foreach ( var i in children ) {
			AddInternal( i );
		}
	}

	[EditorBrowsable( EditorBrowsableState.Never )]
	protected sealed override bool RemoveInternal ( Drawable drawable )
		=> throw new InvalidOperationException( "Cannot remove a 2D drawable from a 3D container" );
	protected virtual void RemoveInternal ( Drawable3D child ) {
		if ( child.Parent == this )
			child.Parent = null;

		children.Remove( child );
		base.RemoveInternal( child );

		subtreeRemoved( this, this, child );
		ChildRemoved?.Invoke( child, this );
	}

	static void subtreeRemoved ( CompositeDrawable3D root, CompositeDrawable3D comp, Drawable3D child ) {
		if ( child is CompositeDrawable3D childComp ) {
			foreach ( var i in childComp.InternalChildren ) {
				subtreeRemoved( root, childComp, i );
			}
		}

		var parent = root;
		while ( parent != null ) {
			parent.SubtreeChildRemoved?.Invoke( child, comp );
			parent = parent.Parent;
		}
	}

	protected void RemoveRangeInternal ( IEnumerable<Drawable3D> children ) {
		foreach ( var i in children ) {
			RemoveInternal( i );
		}
	}

	protected override void ClearInternal ( bool disposeChildren = true ) {
		while ( children.Count != 0 ) {
			var child = children[^1];

			RemoveInternal( child );
			children.RemoveAt( children.Count - 1 );
			if ( disposeChildren )
				child.Dispose();
		}
	}

	protected override void InvalidateMatrix () {
		base.InvalidateMatrix();
		foreach ( var i in children ) {
			i.TryInvalidateMatrix();
		}
	}

	public delegate void HierarchyModifiedHandler ( Drawable3D child, CompositeDrawable3D? parent );
	public event HierarchyModifiedHandler? ChildAdded;
	public event HierarchyModifiedHandler? ChildRemoved;
	public event HierarchyModifiedHandler? SubtreeChildAdded;
	public event HierarchyModifiedHandler? SubtreeChildRemoved;
}
