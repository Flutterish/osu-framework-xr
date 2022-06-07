using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
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
	protected override void UpdateAfterChildren () {
		base.UpdateAfterChildren();

		using ( var buffer = tripleBuffer.Get( UsageType.Write ) ) {
			var node = contentDrawNodes[buffer.Index] = GenerateDrawNodeSubtree( Content, (ulong)buffer.FrameId, buffer.Index, false );
		}
	}

	PanelDrawNode? singleDrawNode;
	protected sealed override DrawNode3D? CreateDrawNode3D ()
		=> singleDrawNode ??= CreatePanelDrawNode();
	protected virtual PanelDrawNode CreatePanelDrawNode ()
		=> new( this );

	FrameBuffer frameBuffer = new();
	AttributeArray VAO = new();
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
				LinkAttributeArray( Mesh, Material );
			}

			Material.Bind();
			Material.Set( "tex", FrameBuffer.Texture );
			Material.Shader.SetUniform( "mMatrix", ref Matrix );
			Material.Shader.SetUniform( "gProj", ( (BasicDrawContext)ctx! ).ProjectionMatrix ); // TODO extract this
			Mesh.Draw();
		}
	}
}
