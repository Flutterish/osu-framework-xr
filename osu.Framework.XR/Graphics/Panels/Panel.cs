﻿using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Physics;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics.Panels;

/// <summary>
/// A 3D panel on which 2D content can be rendered and interacted with through virtual input.
/// You need to manually set the size of content, or set its auto size axes
/// </summary>
public partial class Panel : Drawable3D, IHasCollider {
	/// <summary>
	/// The size of the contained 2D drawable content.
	/// </summary>
	public Vector2 ContentSize {
		get => Content.Size;
		set => Content.Size = value;
	}

	/// <summary>
	/// The auto size axes of the contained 2D drawable content.
	/// </summary>
	public Axes ContentAutoSizeAxes {
		get => Content.AutoSizeAxes;
		set => Content.AutoSizeAxes = value;
	}

	Vector2 lastContentDrawSize;
	public Vector2 ContentDrawSize => Content.DrawSize;

	[Cached( type: typeof( ISafeArea ) )]
	public readonly RootContainer Content;
	public Panel () { // TODO penels should/could have a virtual game host in order to manipulate textinput, clipboard etc.
		AddInternal( Content = CreateRootContainer() );
		colliderMesh = new TransformedBasicMesh( Mesh );
	}
	protected virtual RootContainer CreateRootContainer ()
		=> new();
	public class RootContainer : VirtualInputManager, ISafeArea, IDrawable {
		// this ensures that the panel is the "root node" for cases like buffered containers which clip their size to the root node
		CompositeDrawable? IDrawable.Parent => null;

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

		public RootContainer () {
			RelativeSizeAxes = Axes.None;
		}
	}

	public readonly BasicMesh Mesh = new();
	protected readonly Cached MeshCache = new();
	protected Material Material { get; private set; } = null!;
	protected virtual Material GetDefaultMaterial ( MaterialStore materials )
		=> materials.GetNew( "unlit_panel" );

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		Material ??= GetDefaultMaterial( materials );
	}

	protected override void Update () {
		base.Update();

		colliderMesh.Matrix = Matrix;

		if ( !MeshCache.IsValid ) {
			Mesh.Clear();
			RegenrateMesh();
			colliderMesh.InvalidateAll();
			Mesh.CreateFullUpload().Enqueue();
			MeshCache.Validate();
		}

		if ( lastContentDrawSize != ContentDrawSize ) {
			lastContentDrawSize = ContentDrawSize;
		}
		// we need to update the 2d content draw nodes
		Invalidate( Invalidation.DrawNode );
	}

	protected virtual void RegenrateMesh () {
		Mesh.AddQuad( new Quad3 {
			TL = new Vector3( -1, 1, 0 ),
			TR = new Vector3( 1, 1, 0 ),
			BL = new Vector3( -1, -1, 0 ),
			BR = new Vector3( 1, -1, 0 )
		} );
	}

	public Vector2 ContentPositionAt ( int trisIndex, Vector3 position ) {
		var face = ( Mesh as ITriangleMesh ).GetTriangleFace( trisIndex );
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

	TransformedBasicMesh colliderMesh;
	public ITriangleMesh ColliderMesh => colliderMesh;
	public bool IsColliderEnabled { get; set; } = true;
	public ulong PhysicsLayer { get; set; } = 1;
}