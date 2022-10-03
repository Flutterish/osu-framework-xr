using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics;
using System.Reflection;

namespace osu.Framework.XR.Graphics.Panels;

public partial class Panel {
	// yuck, internal
	static MethodInfo generateDrawNodeSubtree = typeof( Drawable ).GetMethod( "GenerateDrawNodeSubtree", BindingFlags.Instance | BindingFlags.NonPublic )!;
	public static DrawNode? GenerateDrawNodeSubtree ( Drawable drawable, ulong frameId, int treeIndex, bool forceNewDrawNode )
		=> generateDrawNodeSubtree.Invoke( drawable, new object[] { frameId, treeIndex, forceNewDrawNode } ) as DrawNode;

	ulong frameId = 0;

	protected sealed override DrawNode3D? CreateDrawNode3D ( int index )
		=> CreatePanelDrawNode( index );
	protected virtual PanelDrawNode CreatePanelDrawNode ( int index )
		=> new( this, index );

	protected IFrameBuffer? FrameBuffer; // shared data
	AttributeArray VAO = new();
	protected class PanelDrawNode : DrawNode3D {
		protected DrawNode? SourceDrawNode { get; private set; }

		new protected Panel Source => (Panel)base.Source;
		protected readonly AttributeArray VAO;
		protected readonly BasicMesh Mesh;
		protected readonly Material Material;
		protected IFrameBuffer? FrameBuffer {
			get => Source.FrameBuffer;
			set => Source.FrameBuffer = value;
		}
		protected Matrix4 Matrix;
		protected Vector2 Size;
		protected readonly int SubtreeIndex;
		public PanelDrawNode ( Panel source, int index ) : base( source ) {
			VAO = source.VAO;
			Mesh = source.Mesh;
			Material = source.Material;
			SubtreeIndex = index;
		}

		protected override void UpdateState () {
			Matrix = Source.Matrix;
			Size = Source.ContentDrawSize;

			Material.UpdateProperties( SubtreeIndex );
			SourceDrawNode = GenerateDrawNodeSubtree( Source.Content, Source.frameId++, SubtreeIndex, false );
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			SwitchTo2DContext( renderer );
			FrameBuffer ??= renderer.CreateFrameBuffer( new[] { RenderBufferFormat.D32S8 } );
			FrameBuffer.Size = Size;
			FrameBuffer.Bind();
			renderer.PushMaskingInfo( new MaskingInfo {
				ScreenSpaceAABB = new( 0, 0, (int)Size.X, (int)Size.Y ),
				MaskingRect = new( 0, 0, Size.X, Size.Y ),
				ToMaskingSpace = Matrix3.Identity,
				BlendRange = 1,
				AlphaExponent = 1,
				CornerExponent = 2.5f
			}, true );
			renderer.PushViewport( new( 0, 0, (int)Size.X, (int)Size.Y ) );
			renderer.PushOrtho( new( 0, 0, Size.X, Size.Y ) );
			renderer.PushDepthInfo( new() );
			renderer.PushStencilInfo( StencilInfo.Default );
			renderer.PushScissorState( true );
			renderer.PushScissor( new( 0, 0, (int)Size.X, (int)Size.Y ) );
			renderer.PushScissorOffset( Vector2I.Zero );
			renderer.Clear( new( colour: Color4.Transparent ) );

			SourceDrawNode?.Draw( renderer );

			renderer.PopScissorOffset();
			renderer.PopScissor();
			renderer.PopScissorState();
			renderer.PopStencilInfo();
			renderer.PopDepthInfo();
			renderer.PopOrtho();
			renderer.PopViewport();
			renderer.PopMaskingInfo();
			FrameBuffer.Unbind();

			if ( VAO.Bind() ) {
				LinkAttributeArray( Mesh, Material );
			}

			Material.Bind( SubtreeIndex );
			Material.Shader.SetUniform( "tex", FrameBuffer.Texture );
			Material.Shader.SetUniform( "mMatrix", ref Matrix );
			Mesh.Draw();
		}
	}
}
