﻿using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Physics;
using osuTK.Graphics;
using System.Collections;

namespace osu.Framework.XR.Graphics.Panels;

/// <summary>
/// A 3D panel on which 2D content can be rendered and interacted with through virtual input.
/// You need to manually set the size of content, or set its auto size axes
/// </summary>
public partial class Panel : Drawable3D, IHasCollider, IEnumerable<Drawable> {
	/// <summary>
	/// The size of the contained 2D drawable content.
	/// </summary>
	public Vector2 ContentSize {
		get => Content.Size;
		set => Content.Size = value;
	}

	/// <inheritdoc cref="ContentSize"/>
	public override Vector2 Size { 
		get => ContentSize; 
		set => ContentSize = value; 
	}

	/// <summary>
	/// The auto size axes of the contained 2D drawable content.
	/// </summary>
	public Axes ContentAutoSizeAxes {
		get => Content.AutoSizeAxes;
		set => Content.AutoSizeAxes = value;
	}

	/// <inheritdoc cref="ContentAutoSizeAxes"/>
	new public Axes AutoSizeAxes { 
		get => ContentAutoSizeAxes; 
		set => ContentAutoSizeAxes = value;
	}

	/// <summary>
	/// The 2D children
	/// </summary>
	public IReadOnlyList<Drawable> Children {
		get => Content.Children;
		set => Content.Children = value;
	}

	/// <summary>
	/// Gets or sets the only 2D child
	/// </summary>
	public Drawable Child {
		get => Content.Child;
		set => Content.Child = value;
	}

	/// <summary>
	/// Sets all 2D children to elements contained in the enumerable
	/// </summary>
	public IEnumerable<Drawable> ChildrenEnumerable {
		set => Content.ChildrenEnumerable = value;
	}

	/// <summary>
	/// Adds a child to 2D content
	/// </summary>
	public void Add ( Drawable drawable ) {
		Content.Add( drawable );
	}

	/// <summary>
	/// Adds a range of children to 2D content
	/// </summary>
	public void AddRange ( IEnumerable<Drawable> range ) {
		Content.AddRange( range );
	}

	/// <summary>
	/// Removes a given child from 2D content
	/// </summary>
	public bool Remove ( Drawable drawable, bool disposeImmediately ) {
		return Content.Remove( drawable, disposeImmediately );
	}

	/// <summary>
	/// Removes a range of children from 2D content
	/// </summary>
	public void RemoveRange ( IEnumerable<Drawable> range, bool disposeImmediately ) {
		Content.RemoveRange( range, disposeImmediately );
	}

	/// <summary>
	/// Removes all children from 2D content
	/// </summary>
	public void Clear ( bool disposeChildren ) {
		Content.Clear( disposeChildren );
	}

	/// <summary>
	/// Removes all children from 2D content
	/// </summary>
	public void Clear () {
		Content.Clear();
	}

	public IEnumerator<Drawable> GetEnumerator () {
		return ( (IEnumerable<Drawable>)Content ).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator () {
		return ( (IEnumerable)Content ).GetEnumerator();
	}

	Vector2 lastContentDrawSize;
	public Vector2 ContentDrawSize => Content.DrawSize;
	new public Vector2 DrawSize => ContentDrawSize;

	[Cached( type: typeof( ISafeArea ) )]
	public readonly RootContainer Content;
	public Panel () {
		AddInternal( Content = CreateRootContainer() );
		colliderMesh = new TransformedTriangleMesh( Mesh, () => Matrix );
	}
	protected virtual RootContainer CreateRootContainer ()
		=> new();
	public partial class RootContainer : VirtualInputManager, ISafeArea, IDrawable {
		// this ensures that the panel is the "root node" for cases like buffered containers which clip their size to the root node
		CompositeDrawable? IDrawable.Parent => null;

		PlatformActionContainer platformActions;
		protected override Container<Drawable> Content => platformActions;

		public override DrawInfo DrawInfo => new( Matrix3.Identity, Matrix3.Identity );
		public override DrawColourInfo DrawColourInfo => new DrawColourInfo( Colour.MultiplyAlpha( Alpha ), null );

		public Quad ExpandRectangleToSpaceOfOtherDrawable ( IDrawable other ) {
			return ToSpaceOfOtherDrawable( DrawRectangle, other );
		}

		private readonly BindableSafeArea safeArea = new BindableSafeArea();
		public RectangleF AvailableNonSafeSpace => DrawRectangle;
		public BindableSafeArea SafeAreaPadding => safeArea;
		EdgeEffectParameters IContainer.EdgeEffect { get => EdgeEffect; set => EdgeEffect = value; }
		Vector2 IContainer.RelativeChildSize { get => RelativeChildSize; set => RelativeChildSize = value; }
		Vector2 IContainer.RelativeChildOffset { get => RelativeChildOffset; set => RelativeChildOffset = value; }

		public override Vector2 Size { 
			get => base.Size; 
			set => base.Size = platformActions.Size = value; 
		}

		new public Axes AutoSizeAxes { 
			get => base.AutoSizeAxes; 
			set => base.AutoSizeAxes = platformActions.AutoSizeAxes = value;
		}
		public override Axes RelativeSizeAxes { 
			get => base.RelativeSizeAxes; 
			set {
				if ( platformActions is null )
					return;

				base.RelativeSizeAxes = platformActions.RelativeSizeAxes = value;
			}
		}

		public RootContainer () {
			AddInternal( platformActions = new() );
			RelativeSizeAxes = Axes.None;

			ForceLocalVertexBatch = true;
		}

		public override bool UpdateSubTree () {
			var v = base.UpdateSubTree();
			UpdateSubTreeMasking( this, ScreenSpaceDrawQuad.AABBFloat );
			return v;
		}
	}

	public readonly BasicMesh Mesh = new();
	readonly Cached meshCache = new();
	protected void InvalidateMesh () => meshCache.Invalidate();
	protected Material Material { get; private set; } = null!;
	protected virtual Material GetDefaultMaterial ( MaterialStore materials )
		=> materials.GetNew( MaterialNames.UnlitPanel );

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		Material ??= GetDefaultMaterial( materials );
		if ( colour is Color4 color )
			Material.SetIfDefault( Material.StandardTintName, color );
	}

	Color4? colour = null;
	override public ColourInfo Colour {
		get => Tint;
		set => Tint = value.TopLeft;
	}
	public override Color4 Tint {
		get => Material?.Get<Color4>( Material.StandardTintName ) ?? colour ?? Color4.White;
		set {
			if ( Tint == value )
				return;

			colour = value;
			Material?.Set( Material.StandardTintName, value );
			Invalidate( Invalidation.DrawNode );
		}
	}
	override public float Alpha {
		get => Tint.A;
		set => Tint = Tint with { A = value };
	}

	protected override void InvalidateMatrix () {
		base.InvalidateMatrix();
		colliderMesh.InvalidateMatrix();
	}

	// shared data
	ulong meshId;
	ulong linkedMeshId;
	protected virtual bool ClearMeshOnInvalidate => true;
	protected override void UpdateAfterChildren () {
		base.UpdateAfterChildren();

		if ( !meshCache.IsValid ) {
			if ( ClearMeshOnInvalidate ) 
				Mesh.Clear();
			RegenrateMesh();
			colliderMesh.InvalidateAll();
			Mesh.CreateFullUpload().Enqueue();
			meshCache.Validate();
			meshId++;
		}

		if ( lastContentDrawSize != ContentDrawSize ) {
			lastContentDrawSize = ContentDrawSize;
		}
		// we need to update the 2d content draw nodes
		Invalidate( Invalidation.DrawNode );
	}

	/// <summary>
	/// Regenrate mesh after it's been invalidated though <see cref="InvalidateMesh()"/>
	/// </summary>
	/// <remarks>
	/// Note that <see cref="ContentDrawSize"/> might be a fractional value, while 
	/// rendered content is snapped to the truncated pixel value
	/// </remarks>
	protected virtual void RegenrateMesh () {
		Mesh.AddQuad( new Quad3 {
			TL = new Vector3( -1, 1, 0 ),
			TR = new Vector3( 1, 1, 0 ),
			BL = new Vector3( -1, -1, 0 ),
			BR = new Vector3( 1, -1, 0 )
		} );
	}

	public Vector2 GlobalSpaceContentPositionAt ( int trisIndex, Vector3 position ) {
		var face = ColliderMesh.GetTriangleFace( trisIndex );
		var barycentric = Triangles.BarycentricFast( face, position );
		var tris = Mesh.GetTriangleIndices( trisIndex );
		var textureCoord =
			  Mesh.VertexBuffer.Data[(int)tris.indexA].UV * barycentric.X
			+ Mesh.VertexBuffer.Data[(int)tris.indexB].UV * barycentric.Y
			+ Mesh.VertexBuffer.Data[(int)tris.indexC].UV * barycentric.Z;
		return new Vector2( Content.DrawWidth * textureCoord.X, Content.DrawHeight * ( 1 - textureCoord.Y ) );
	}

	public Vector2 ModelSpaceContentPositionAt ( int trisIndex, Vector3 position ) {
		var face = Mesh.GetTriangleFace( trisIndex );
		var barycentric = Triangles.BarycentricFast( face, position );
		var tris = Mesh.GetTriangleIndices( trisIndex );
		var textureCoord =
			  Mesh.VertexBuffer.Data[(int)tris.indexA].UV * barycentric.X
			+ Mesh.VertexBuffer.Data[(int)tris.indexB].UV * barycentric.Y
			+ Mesh.VertexBuffer.Data[(int)tris.indexC].UV * barycentric.Z;
		return new Vector2( Content.DrawWidth * textureCoord.X, Content.DrawHeight * ( 1 - textureCoord.Y ) );
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			VAO.Dispose();
			Mesh.Dispose();
			FrameBuffer?.Dispose();
		}

		base.Dispose( isDisposing );
	}

	// TODO this probably shouldnt be here?
	public void CreateUVTest ( int tilesX, int tilesY, int? seed = null ) {
		Random rng = seed is int i ? new( i ) : new();

		var tileWidth = ContentDrawSize.X / tilesX;
		var tileHeight = ContentDrawSize.Y / tilesY;
		var size = new Vector2( tileWidth, tileHeight );
		for ( var x = 0; x < tilesX; x++ ) {
			for ( var y = 0; y < tilesY; y++ ) {
				var color = Color4.FromHsv( new( rng.NextSingle(), 1, 1, 1 ) );

				Content.Add( new Box {
					Size = size,
					X = tileWidth * x,
					Y = tileHeight * y,
					Colour = color
				} );
				Content.Add( new SpriteText {
					X = tileWidth * x + tileWidth / 2,
					Y = tileHeight * y + tileHeight / 2,
					Origin = Anchor.Centre,
					Text = $"{x}:{y}"
				} );
			}
		}
	}

	TransformedTriangleMesh colliderMesh;
	public ITriangleMesh ColliderMesh => colliderMesh;
	public bool IsColliderEnabled { get; set; } = true;
	public ulong PhysicsLayer { get; set; } = 1;
}
