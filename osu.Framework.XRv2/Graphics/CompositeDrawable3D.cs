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
			throw new InvalidOperationException( $"Cannot add a {nameof( Drawable3D )} into multiple containers" );

		child.Parent = this;
		children.Add( child );
		base.AddInternal( child );
		if ( !ShouldUpdateChildrenLife )
			MakeChildAlive( child );

		ChildAdded?.Invoke( child, this, true );
	}

	protected void AddRangeInternal ( IEnumerable<Drawable3D> children ) {
		foreach ( var i in children ) {
			AddInternal( i );
		}
	}

	[EditorBrowsable( EditorBrowsableState.Never )]
	protected sealed override bool RemoveInternal ( Drawable drawable, bool disposeImmediately )
		=> throw new InvalidOperationException( "Cannot remove a 2D drawable from a 3D container" );
	protected virtual void RemoveInternal ( Drawable3D child, bool disposeImmediately ) {
		if ( child.Parent == this )
			child.Parent = null;

		children.Remove( child );
		base.RemoveInternal( child, disposeImmediately );

		ChildRemoved?.Invoke( child, this, true );
	}

	protected void RemoveRangeInternal ( IEnumerable<Drawable3D> children, bool disposeImmediately ) {
		foreach ( var i in children ) {
			RemoveInternal( i, disposeImmediately );
		}
	}

	protected override void ClearInternal ( bool disposeChildren = true ) {
		while ( children.Count != 0 ) {
			var child = children[^1];

			RemoveInternal( child, disposeChildren );
			children.RemoveAt( children.Count - 1 );
		}
	}

	/// <summary>
	/// Whether or not to invoke <see cref="UpdateChildrenLife"/>. 
	/// <see cref="Drawable3D"/>s do not manage lifetime by default, so this this is <see langword="false"/> by default
	/// </summary>
	protected virtual bool ShouldUpdateChildrenLife => false;
	protected override bool UpdateChildrenLife () {
		return ShouldUpdateChildrenLife ? base.UpdateChildrenLife() : false;
	}

	protected override void InvalidateMatrix () {
		base.InvalidateMatrix();
		foreach ( var i in children ) {
			i.TryInvalidateMatrix();
		}
	}

	/// <param name="child">The affected drawable</param>
	/// <param name="parent">The containing drawable</param>
	/// <param name="isImmediate">Whether this is a direct action (<see langword="true"/>) or a result of another modification (<see langword="false"/>)</param>
	public delegate void HierarchyModifiedHandler ( Drawable3D child, CompositeDrawable3D? parent, bool isImmediate );
	// TODO we don't really wanna expose the subtree, but these events allow to scan it (and are more or less required for rendering).
	// we might want to give drawables the ability to 'consent' to them or their subtrees being seen
	// by a given type of scanner
	public event HierarchyModifiedHandler? ChildAdded;
	public event HierarchyModifiedHandler? ChildRemoved;

	public SubtreeModifiedSubscription SubscribeSubtreeModified ( HierarchyModifiedHandler added, HierarchyModifiedHandler removed ) {
		HierarchyModifiedHandler addedVisitor = null!;
		HierarchyModifiedHandler removedVisitor = null!;

		addedVisitor = ( child, parent, immediate ) => {
			added( child, parent, immediate );

			if ( child is CompositeDrawable3D comp ) {
				comp.ChildAdded += addedVisitor;
				comp.ChildRemoved += removedVisitor;
				foreach ( var i in comp.children ) {
					addedVisitor( i, comp, false );
				}
			}
		};
		removedVisitor = ( child, parent, immediate ) => {
			if ( child is CompositeDrawable3D comp ) {
				foreach ( var i in comp.children.Reverse<Drawable3D>() ) {
					removedVisitor( i, comp, false );
				}
				comp.ChildRemoved -= removedVisitor;
				comp.ChildAdded -= addedVisitor;
			}

			removed( child, parent, immediate );
		};

		ChildAdded += addedVisitor;
		ChildRemoved += removedVisitor;

		foreach ( var i in children ) {
			addedVisitor( i, this, true );
		}

		return new() { AddedVisitor = addedVisitor, RemovedVisitor = removedVisitor };
	}

	public void UnsubscribeSubtreeModified ( SubtreeModifiedSubscription subscription ) {
		ChildAdded -= subscription.AddedVisitor;
		ChildRemoved -= subscription.RemovedVisitor;

		foreach ( var child in children ) {
			if ( child is CompositeDrawable3D comp ) {
				comp.UnsubscribeSubtreeModified( subscription );
			}
		}
	}
}

public readonly struct SubtreeModifiedSubscription {
	public CompositeDrawable3D.HierarchyModifiedHandler AddedVisitor { get; init; }
	public CompositeDrawable3D.HierarchyModifiedHandler RemovedVisitor { get; init; }
}