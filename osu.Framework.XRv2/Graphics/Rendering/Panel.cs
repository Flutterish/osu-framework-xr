using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;
using osuTK.Graphics;
using System.Reflection;

namespace osu.Framework.XR.Graphics.Rendering;

/// <summary>
/// A 3D panel on which 2D content can be rendered and interacted with through virtual input.
/// You need to manually set the size of content, or set its auto size axes
/// </summary>
public class Panel : Drawable3D {
	/// <summary>
	/// The size of the contained 2D drawable content.
	/// </summary>
	public Vector2 ContentSize {
		get => Content.Size;
		set => Content.Size = value;
	}

	Vector2 lastContentDrawSize;
	public Vector2 ContentDrawSize => Content.DrawSize;
	public override DrawInfo DrawInfo => new( Matrix3.Identity, Matrix3.Identity );

	[Cached( type: typeof( ISafeArea ) )]
	protected class RootContainer : Container, ISafeArea, IDrawable {
		// this ensures that the panel is the "root node" for cases like buffered containers which clip their size to the root node
		CompositeDrawable? IDrawable.Parent => null;

		public Quad ExpandRectangleToSpaceOfOtherDrawable ( IDrawable other ) {
			return ToSpaceOfOtherDrawable( DrawRectangle, other );
		}

		private readonly BindableSafeArea safeArea = new BindableSafeArea();
		public RectangleF AvailableNonSafeSpace => DrawRectangle;
		public BindableSafeArea SafeAreaPadding => safeArea;
		EdgeEffectParameters IContainer.EdgeEffect { get => EdgeEffect; set => EdgeEffect = value; }
		Vector2 IContainer.RelativeChildSize { get => RelativeChildSize; set => RelativeChildSize = value; }
		Vector2 IContainer.RelativeChildOffset { get => RelativeChildOffset; set => RelativeChildOffset = value; }
	}
	public readonly Container Content;
	public Panel () {
		AddInternal( Content = new RootContainer() );
	}

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		Material = materials.GetNew( "unlit_panel" );
	}

	TripleBuffer<Drawable> tripleBuffer = new();
	DrawNode[] contentDrawNodes = new DrawNode[3];
	PanelDrawNode? singleDrawNode;
	protected sealed override DrawNode3D? CreateDrawNode3D ()
		=> singleDrawNode ??= CreatePanelDrawNode();
	protected virtual PanelDrawNode CreatePanelDrawNode ()
		=> new( this );

	// yuck, internal
	static MethodInfo generateDrawNodeSubtree = typeof( Drawable ).GetMethod( "GenerateDrawNodeSubtree", BindingFlags.Instance | BindingFlags.NonPublic )!;
	public static DrawNode GenerateDrawNodeSubtree ( Drawable drawable, ulong frameId, int treeIndex, bool forceNewDrawNode )
		=> (DrawNode)generateDrawNodeSubtree.Invoke( drawable, new object[] { frameId, treeIndex, forceNewDrawNode } )!;
	protected override void UpdateAfterChildren () {
		base.UpdateAfterChildren();

		using ( var buffer = tripleBuffer.Get( UsageType.Write ) ) {
			var node = contentDrawNodes[buffer.Index] ??= GenerateDrawNodeSubtree( Content, (ulong)buffer.FrameId, buffer.Index, false );
			node.ApplyState();
		}
	}

	FrameBuffer frameBuffer = new();
	AttributeArray VAO = new();
	protected readonly BasicMesh Mesh = new();
	protected Material Material { get; private set; } = null!;
	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			VAO.Dispose();
			Mesh.Dispose();
			frameBuffer.Dispose();
		}

		base.Dispose( isDisposing );
	}

	protected Cached MeshCache = new();
	protected override void Update () {
		base.Update();

		if ( !MeshCache.IsValid ) {
			Mesh.Clear();
			RegenrateMesh();
			Mesh.CreateFullUpload().Enqueue();
			MeshCache.Validate();
		}

		if ( lastContentDrawSize != ContentDrawSize ) {
			lastContentDrawSize = ContentDrawSize;
			Invalidate( Invalidation.DrawNode );
		}
	}

	protected virtual void RegenrateMesh () {
		Mesh.AddQuad( new Quad3 {
			TL = new Vector3( -1, 1, 0 ),
			TR = new Vector3( 1, 1, 0 ),
			BL = new Vector3( -1, -1, 0 ),
			BR = new Vector3( 1, -1, 0 )
		} );
	}

	protected class PanelDrawNode : DrawNode3D {
		new protected Panel Source => (Panel)base.Source;
		AttributeArray VAO;
		protected readonly BasicMesh Mesh;
		protected readonly Material Material;
		protected readonly FrameBuffer FrameBuffer = new();
		protected Matrix4 Matrix;
		protected Vector2 Size;
		public PanelDrawNode ( Panel source ) : base( source ) {
			VAO = source.VAO;
			Mesh = source.Mesh;
			Material = source.Material;
			FrameBuffer = source.frameBuffer;
		}

		protected override void UpdateState () {
			Matrix = Source.Matrix;
			Size = Source.ContentDrawSize;
		}

		public override void Draw ( object? ctx = null ) {
			SwitchTo2DContext();
			FrameBuffer.Size = Size;
			FrameBuffer.Bind();
			GLWrapper.PushMaskingInfo( new MaskingInfo {
				ScreenSpaceAABB = new( 0, 0, (int)Size.X, (int)Size.Y ),
				MaskingRect = new( 0, 0, Size.X, Size.Y ),
				ToMaskingSpace = Matrix3.Identity,
				BlendRange = 1,
				AlphaExponent = 1
			}, true );
			GLWrapper.PushViewport( new( 0, 0, (int)Size.X, (int)Size.Y ) );
			GLWrapper.PushOrtho( new( 0, 0, Size.X, Size.Y ) );
			GLWrapper.PushDepthInfo( new() );
			GLWrapper.PushScissorState( false );
			GLWrapper.Clear( new( colour: Color4.Transparent ) );

			using ( var buffer = Source.tripleBuffer.Get( UsageType.Read ) ) {
				var node = Source.contentDrawNodes[buffer.Index];
				node?.Draw( null );
			}

			GLWrapper.PopScissorState();
			GLWrapper.PopDepthInfo();
			GLWrapper.PopOrtho();
			GLWrapper.PopViewport();
			GLWrapper.PopMaskingInfo();
			FrameBuffer.Unbind();

			if ( VAO.Bind() ) {
				Mesh.ElementBuffer!.Bind();
				Mesh.VertexBuffers[0].Link( Material.Shader, new int[] { Material.Shader.GetAttrib( "aPos" ), Material.Shader.GetAttrib( "aUv" ) } );
			}

			Material.Bind();
			Material.Set( "tex", FrameBuffer.Texture );
			Material.Shader.SetUniform( "mMatrix", ref Matrix );
			Material.Shader.SetUniform( "gProj", ( (BasicDrawContext)ctx! ).ProjectionMatrix ); // TODO extract this
			Mesh.Draw();
		}
	}
}
