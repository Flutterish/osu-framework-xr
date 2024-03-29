﻿using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Collections;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Rendering;

namespace osu.Framework.XR.Graphics.Containers;

/// <summary>
/// Renders a batch of drawables which share the same <see cref="IAttributeArray"/>. The children must be
/// <see cref="IUnrenderable"/> so they arent drawn with the main pipeline - they will share the same
/// <see cref="Drawable3D.RenderStage"/> as this batch, as they are drawn with it
/// </summary>
public abstract partial class BatchDrawable<Tdrawable, Tnode> : Container3D<Tdrawable> where Tdrawable : Drawable3D, IUnrenderable where Tnode : DrawNode3D {
	AttributeArray VAO = new();
	ulong subtreeUpdateID = 1;
	protected abstract override BatchDrawNode CreateDrawNode3D ( int subtreeIndex );

	public override void Add ( Tdrawable child ) {
		base.Add( child );
		subtreeUpdateID++;
		Invalidate( Invalidation.DrawNode );
	}

	public override void Remove ( Tdrawable child, bool disposeImmediately ) {
		base.Remove( child, disposeImmediately );
		subtreeUpdateID++;
		Invalidate( Invalidation.DrawNode );
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			VAO.Dispose();
		}
		base.Dispose( isDisposing );
	}

	public abstract class BatchDrawNode : DrawNode3D {
		AttributeArray VAO;

		new protected BatchDrawable<Tdrawable, Tnode> Source => (BatchDrawable<Tdrawable, Tnode>)base.Source;
		protected readonly int SubtreeIndex;
		ulong subtreeUpdateID;
		public BatchDrawNode ( BatchDrawable<Tdrawable, Tnode> source, int subtreeIndex ) : base( source ) {
			VAO = source.VAO;
			SubtreeIndex = subtreeIndex;
		}

		List<Tnode> children = new();
		protected ReadOnlySpan<Tnode> Children => children.AsSpan();
		protected override void UpdateState () {
			if ( subtreeUpdateID != Source.subtreeUpdateID ) {
				subtreeUpdateID = Source.subtreeUpdateID;
				children.Clear();
				foreach ( var i in Source.Children ) {
					var node = i.GetDrawNodeAtSubtree( SubtreeIndex );
					if ( node is Tnode n ) {
						node.UpdateNode();
						children.Add( n );
					}
				}
			}
			else {
				foreach ( var i in Children ) {
					if ( i.InvalidationID != i.Source.InvalidationID )
						i.UpdateNode();
				}
			}
		}

		public sealed override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( VAO.Bind() )
				Initialize();

			DrawBatch( ctx );
		}

		protected abstract void Initialize ();
		protected abstract void DrawBatch ( object? ctx = null );
	}
}