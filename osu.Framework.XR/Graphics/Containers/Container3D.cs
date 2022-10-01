using osu.Framework.Lists;

namespace osu.Framework.XR.Graphics.Containers;

public class Container3D : Container3D<Drawable3D> { }

public class Container3D<T> : CompositeDrawable3D where T : Drawable3D {
	protected virtual Container3D<T> Content => this;
	public IReadOnlyList<T> Children => Content == this ? internalChildrenAsT : Content.Children;
	private IReadOnlyList<T> internalChildrenAsT;

	static readonly bool storesDrawable3D = typeof( T ) == typeof( Drawable3D );
	public Container3D () {
		internalChildrenAsT = storesDrawable3D
			? (IReadOnlyList<T>)InternalChildren
			: new LazyList<Drawable3D, T>( InternalChildren, d => (T)d );
	}

	public virtual void Add ( T child ) {
		if ( Content == this )
			AddInternal( child );
		else
			Content.Add( child );
	}

	public void AddRange ( IEnumerable<T> children ) {
		if ( Content == this )
			AddRangeInternal( children );
		else
			Content.AddRange( children );
	}

	public virtual void Remove ( T child, bool disposeImmediately ) {
		if ( Content == this )
			RemoveInternal( child, disposeImmediately );
		else
			Content.Remove( child, disposeImmediately );
	}

	public void RemoveRange ( IEnumerable<T> children, bool disposeImmediately ) {
		if ( Content == this )
			RemoveRangeInternal( children, disposeImmediately );
		else
			Content.RemoveRange( children, disposeImmediately );
	}

	public void Clear ( bool disposeChildren = true ) {
		if ( Content == this )
			ClearInternal( disposeChildren );
		else
			Content.Clear( disposeChildren );
	}
}
