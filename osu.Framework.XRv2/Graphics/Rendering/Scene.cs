using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics.Containers;

namespace osu.Framework.XR.Graphics.Rendering;

[Cached]
public partial class Scene : CompositeDrawable {
	public readonly Container3D Root = new();
	Queue<(Drawable3D drawable, bool added, Enum stage)> drawableQueue = new();
	Queue<(Drawable3D drawable, bool added, Enum stage)> uploadableQueue = new();
	HashSet<Drawable3D> drawables = new();

	Camera? camera;
	public Camera Camera {
		get => camera ??= new();
		set => camera = value;
	}

	public Scene () {
		AddInternal( Root );
		Root.SubtreeChildAdded += ( d, p ) => {
			if ( d is IUnrenderable )
				return;

			drawables.Add( d );
			drawableQueue.Enqueue( (d, true, d.RenderStage) );
			d.RenderStageChanged += onRenderStageChanged;
		};
		Root.SubtreeChildRemoved += ( d, p ) => {
			if ( d is IUnrenderable )
				return;

			drawables.Remove( d );
			drawableQueue.Enqueue( (d, false, d.RenderStage) );
			d.RenderStageChanged -= onRenderStageChanged;
		};

		for ( int i = 0; i < 1000; i++ ) {
			Root.Add( new Model() {
				X = RNG.NextSingle( -5, 5 ),
				Y = RNG.NextSingle( -5, 5 ),
				Z = RNG.NextSingle( -5, 5 )
			} );
		}
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

		Invalidate( Invalidation.DrawNode ); // camera updates are not invalidated
	}

	[BackgroundDependencyLoader]
	private void load ( ShaderManager shaders ) {
		blitShader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
	}
	IShader blitShader = null!;

	public RenderPiepline? GetRenderPiepline ()
		=> singleDrawNode;

	RenderPiepline? singleDrawNode;
	protected sealed override DrawNode CreateDrawNode ()
		=> singleDrawNode ??= CreateRenderPipeline();

	protected virtual RenderPiepline CreateRenderPipeline ()
		=> new RenderPiepline( this );
}
