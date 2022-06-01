using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;

namespace osu.Framework.XR.Graphics.Containers;

[Cached]
public class Scene : CompositeDrawable {
	public readonly Container3D Root = new();
	Queue<(Drawable3D drawable, bool added, Enum stage)> drawableQueue = new();
	Queue<(Drawable3D drawable, bool added, Enum stage)> uploadableQueue = new();
	HashSet<Drawable3D> drawables = new();

	public Scene () {
		AddInternal( Root );
		Root.SubtreeChildAdded += ( d, p ) => {
			drawables.Add( d );
			drawableQueue.Enqueue( (d, true, d.RenderStage) );
			d.RenderStageChanged += onRenderStageChanged;
		};
		Root.SubtreeChildRemoved += ( d, p ) => {
			drawables.Remove( d );
			drawableQueue.Enqueue( (d, false, d.RenderStage) );
			d.RenderStageChanged -= onRenderStageChanged;
		};

		Root.Add( new Model() );
	}

	private void onRenderStageChanged ( Drawable3D drawable, Enum from, Enum to ) {
		drawableQueue.Enqueue( (drawable, false, from) );
		drawableQueue.Enqueue( (drawable, true, to) );
	}

	TripleBuffer<Drawable3D> tripleBuffer = new();
	object uploadMutex = new();
	protected override void UpdateAfterChildren () {
		base.UpdateAfterChildren();

		using ( var write = tripleBuffer.Get( UsageType.Write ) ) {
			foreach ( var i in drawables ) {
				var node = i.GetDrawNodeAtSubtree( write.Index );
				if ( node != null && i.InvalidationID != node.InvalidationID )
					node.UpdateNode();
			}
		}

		lock ( uploadMutex ) {
			while ( drawableQueue.TryDequeue( out var data ) ) {
				uploadableQueue.Enqueue( data );
			}
		}
	}

	[BackgroundDependencyLoader]
	private void load ( ShaderManager shaders ) {
		blitShader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
	}
	IShader blitShader = null!;

	SceneDrawNode? singleDrawNode;
	protected sealed override DrawNode CreateDrawNode ()
		=> singleDrawNode ??= new SceneDrawNode( this );

	class SceneDrawNode : DrawNode, ICompositeDrawNode {
		HashSet<Drawable3D> defaultDrawLayer = new();

		new protected Scene Source => (Scene)base.Source;

		public SceneDrawNode ( Scene source ) : base( source ) {
			frameBuffer = new( new[] { osuTK.Graphics.ES30.RenderbufferInternalFormat.DepthComponent32f } );
		}

		Quad screenSpaceDrawQuad;
		Vector2 size;
		osu.Framework.Graphics.OpenGL.Buffers.FrameBuffer frameBuffer;
		IShader blitShader = null!;
		public override void ApplyState () {
			base.ApplyState();

			screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
			blitShader = Source.blitShader;
			size = Source.DrawSize;
		}

		public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
			lock ( Source.uploadMutex ) {
				while ( Source.uploadableQueue.TryDequeue( out var data ) ) {
					if ( data.added ) {
						defaultDrawLayer.Add( data.drawable );
					}
					else {
						defaultDrawLayer.Remove( data.drawable );
					}
				}
			}

			UploadScheduler.Execute();

			frameBuffer.Size = size;
			frameBuffer.Bind();
			GLWrapper.PushViewport( new( 0, 0, (int)frameBuffer.Size.X, (int)frameBuffer.Size.Y ) );
			GLWrapper.PushScissorState( false );
			GLWrapper.PushDepthInfo( new() );
			GLWrapper.Clear( new( depth: 0 ) );

			using ( var read = Source.tripleBuffer.Get( UsageType.Read ) ) {
				foreach ( var i in defaultDrawLayer ) {
					i.GetDrawNodeAtSubtree( read.Index )?.Draw();
				}
			}

			GL.BindVertexArray( 0 );
			GLWrapper.PopDepthInfo();
			GLWrapper.PopScissorState();
			GLWrapper.PopViewport();
			frameBuffer.Unbind();

			blitShader.Bind();
			frameBuffer.Texture.Bind();
			DrawQuad( frameBuffer.Texture, screenSpaceDrawQuad, DrawColourInfo.Colour );
		}

		public List<DrawNode>? Children { get; set; }
		public bool AddChildDrawNodes => false;
	}
}
