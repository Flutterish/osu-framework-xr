using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics;
using System.Reflection;

namespace osu.Framework.XR.Graphics.Panels;

public partial class Panel {
	TripleBuffer<Drawable> tripleBuffer = new();
	DrawNode[] contentDrawNodes = new DrawNode[3];

	// yuck, internal
	static MethodInfo generateDrawNodeSubtree = typeof( Drawable ).GetMethod( "GenerateDrawNodeSubtree", BindingFlags.Instance | BindingFlags.NonPublic )!;
	public static DrawNode GenerateDrawNodeSubtree ( Drawable drawable, ulong frameId, int treeIndex, bool forceNewDrawNode )
		=> (DrawNode)generateDrawNodeSubtree.Invoke( drawable, new object[] { frameId, treeIndex, forceNewDrawNode } )!;

	ulong frameId = 0;
	protected override void UpdateAfterChildren () {
		base.UpdateAfterChildren();

		using ( var buffer = tripleBuffer.GetForWrite() ) {
			var node = contentDrawNodes[buffer.Index] = GenerateDrawNodeSubtree( Content, frameId, buffer.Index, false );
		}
		frameId++;
	}

	PanelDrawNode? singleDrawNode;
	protected sealed override DrawNode3D? CreateDrawNode3D ()
		=> singleDrawNode ??= CreatePanelDrawNode();
	protected virtual PanelDrawNode CreatePanelDrawNode ()
		=> new( this );

	IFrameBuffer? frameBuffer; // shared data
	AttributeArray VAO = new();
	protected class PanelDrawNode : DrawNode3D {
		new protected Panel Source => (Panel)base.Source;
		AttributeArray VAO;
		protected readonly BasicMesh Mesh;
		protected readonly Material Material;
		protected IFrameBuffer? FrameBuffer {
			get => Source.frameBuffer;
			set => Source.frameBuffer = value;
		}
		protected Matrix4 Matrix;
		protected Vector2 Size;
		public PanelDrawNode ( Panel source ) : base( source ) {
			VAO = source.VAO;
			Mesh = source.Mesh;
			Material = source.Material;
		}

		protected override void UpdateState () {
			Matrix = Source.Matrix;
			Size = Source.ContentDrawSize;
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			SwitchTo2DContext( renderer );
			FrameBuffer ??= renderer.CreateFrameBuffer();
			FrameBuffer.Size = Size;
			FrameBuffer.Bind();
			renderer.PushMaskingInfo( new MaskingInfo {
				ScreenSpaceAABB = new( 0, 0, (int)Size.X, (int)Size.Y ),
				MaskingRect = new( 0, 0, Size.X, Size.Y ),
				ToMaskingSpace = Matrix3.Identity,
				BlendRange = 1,
				AlphaExponent = 1
			}, true );
			renderer.PushViewport( new( 0, 0, (int)Size.X, (int)Size.Y ) );
			renderer.PushOrtho( new( 0, 0, Size.X, Size.Y ) );
			renderer.PushDepthInfo( new() );
			renderer.PushScissorState( false );
			renderer.Clear( new( colour: Color4.Transparent ) );

			using ( var buffer = Source.tripleBuffer.GetForRead() ) {
				var node = Source.contentDrawNodes[buffer.Index];
				node?.Draw( renderer );
			}

			renderer.PopScissorState();
			renderer.PopDepthInfo();
			renderer.PopOrtho();
			renderer.PopViewport();
			renderer.PopMaskingInfo();
			FrameBuffer.Unbind();

			if ( VAO.Bind() ) {
				LinkAttributeArray( Mesh, Material );
			}

			Material.Bind();
			Material.Set( "tex", FrameBuffer.Texture );
			Material.Shader.SetUniform( "mMatrix", ref Matrix );
			Mesh.Draw();
		}
	}
}
