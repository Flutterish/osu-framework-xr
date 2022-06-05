using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.XR.Collections;
using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Materials;

namespace osu.Framework.XR.Graphics.Rendering;

/// <summary>
/// A 3D scene.
/// </summary>
/// <remarks>
/// Resources used by the scene drawables should be created in <see cref="CreateChildDependencies(IReadOnlyDependencyContainer)"/>
/// </remarks>
[Cached]
public partial class Scene : CompositeDrawable {
	public readonly Container3D Root = new();
	Queue<(Drawable3D drawable, bool added, Enum stage)> drawableQueue = new();
	Queue<(Drawable3D drawable, bool added, Enum stage)> uploadableQueue = new();
	HashList<Drawable3D> drawables = new();

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
	}

	private void onRenderStageChanged ( Drawable3D drawable, Enum from, Enum to ) {
		drawableQueue.Enqueue( (drawable, false, from) );
		drawableQueue.Enqueue( (drawable, true, to) );
	}

	public void Add ( Drawable3D drawable )
		=> Root.Add( drawable );
	public void AddRange ( IEnumerable<Drawable3D> drawables )
		=> Root.AddRange( drawables );
	public void Remove ( Drawable3D drawable )
		=> Root.Remove( drawable );
	public void RemoveRange ( IEnumerable<Drawable3D> drawables )
		=> Root.RemoveRange( drawables );
	public void Clear ( bool disposeChildren = true )
		=> Root.Clear( disposeChildren );

	TripleBuffer<Drawable3D> tripleBuffer = new();
	object uploadMutex = new();
	protected override void UpdateAfterChildren () {
		base.UpdateAfterChildren();

		using ( var write = tripleBuffer.Get( UsageType.Write ) ) {
			foreach ( var i in drawables.AsSpan() ) {
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

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );
		var store = parent.Get<Game>().Resources;
		var materials = new MaterialStore( new NamespacedResourceStore<byte[]>( store, "Resources/Shaders" ) );
		var textures = new TextureStore(
			parent.Get<GameHost>().CreateTextureLoaderStore( new NamespacedResourceStore<byte[]>( store, "Resources/Textures" ) ),
			useAtlas: true,
			osuTK.Graphics.ES30.All.Nearest,
			manualMipmaps: false,
			scaleAdjust: 1
		);
		deps.Cache( materials );
		deps.Cache( textures );
		return base.CreateChildDependencies( deps );
	}

	public RenderPiepline? GetRenderPiepline ()
		=> singleDrawNode;

	RenderPiepline? singleDrawNode;
	protected sealed override DrawNode CreateDrawNode ()
		=> singleDrawNode ??= CreateRenderPipeline();

	protected virtual RenderPiepline CreateRenderPipeline ()
		=> new( this );
}
