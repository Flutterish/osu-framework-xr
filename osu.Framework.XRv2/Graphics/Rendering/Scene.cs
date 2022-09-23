using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.XR.Collections;
using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics;

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
		onRenderStageChangedDelegate = onRenderStageChanged;
		AddInternal( Root );
		Root.SubtreeChildAdded += ( d, p ) => {
			if ( d is IUnrenderable )
				return;

			drawables.Add( d );
			drawableQueue.Enqueue( (d, true, d.RenderStage) );
			d.RenderStageChanged += onRenderStageChangedDelegate;
		};
		Root.SubtreeChildRemoved += ( d, p ) => {
			if ( d is IUnrenderable )
				return;

			drawables.Remove( d );
			drawableQueue.Enqueue( (d, false, d.RenderStage) );
			d.RenderStageChanged -= onRenderStageChangedDelegate;
		};
	}

	// cached delegate to avoid allocs
	Drawable3D.RenderStageChangedHandler onRenderStageChangedDelegate;
	private void onRenderStageChanged ( Drawable3D drawable, Enum from, Enum to ) {
		drawableQueue.Enqueue( (drawable, false, from) );
		drawableQueue.Enqueue( (drawable, true, to) );
	}

	public void Add ( Drawable3D drawable )
		=> Root.Add( drawable );
	public void AddRange ( IEnumerable<Drawable3D> drawables )
		=> Root.AddRange( drawables );
	public void Remove ( Drawable3D drawable, bool disposeImmediately )
		=> Root.Remove( drawable, disposeImmediately );
	public void RemoveRange ( IEnumerable<Drawable3D> drawables, bool disposeImmediately )
		=> Root.RemoveRange( drawables, disposeImmediately );
	public void Clear ( bool disposeChildren = true )
		=> Root.Clear( disposeChildren );

	TripleBuffer<Drawable3D> tripleBuffer = new();
	object uploadMutex = new();
	protected override void UpdateAfterChildren () {
		base.UpdateAfterChildren();

		using ( var write = tripleBuffer.GetForWrite() ) {
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

	public MaterialStore MaterialStore { get; private set; } = null!;
	protected virtual ResourceStore<byte[]>? CreateMaterialStoreSource ( IReadOnlyDependencyContainer deps ) {
		return null;
	}

	GameHost? host;
	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );
		var store = parent.Get<Game>().Resources;
		host = parent.Get<GameHost>();
		var renderer = parent.Get<IRenderer>();
		var materials = MaterialStore = new MaterialStore( new ResourceStore<byte[]>( new[] {
			CreateMaterialStoreSource( deps ) ?? new NamespacedResourceStore<byte[]>( store, "Resources/Shaders" ),
			new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof(Scene).Assembly ), "Resources/Shaders" )
		} ) );
		var textures = new TextureStore(
			renderer,
			parent.Get<GameHost>().CreateTextureLoaderStore( new NamespacedResourceStore<byte[]>( store, "Resources/Textures" ) ),
			useAtlas: true,
			TextureFilteringMode.Nearest,
			manualMipmaps: false,
			scaleAdjust: 1
		);
		materials.AddDescriptor( "unlit", new MaterialDescriptor()
			.SetAttribute( "aPos", MeshDescriptor.Position )
			.SetAttribute( "aUv", MeshDescriptor.UV )
			.SetUniform( "tex", renderer.WhitePixel )
			.SetUniform( "subImage", renderer.WhitePixel.GetTextureRect() )
			.SetUniform( "tint", Color4.White )
			.SetOnBind( ( m, store ) => {
				m.Shader.SetUniform( "gProj", store.GetGlobalProperty<Matrix4>( "gProj" ) );
			} )
		);
		materials.SetGlobalProperty( "lightPos", Vector3.Zero );
		materials.AddDescriptor( "lit", new MaterialDescriptor( materials.GetDescriptor( "unlit" ) )
			.SetAttribute( "aNorm", MeshDescriptor.Normal )
			.SetUniform( "lightColor", Vector3.One )
			.SetUniform( "specularStr", 0.5f )
			.SetUniform( "ambientStr", 0.1f )
			.SetUniform( "specularExp", 32f )
			.AddOnBind( ( m, store ) => {
				m.Shader.SetUniform( "viewPos", store.GetGlobalProperty<Vector3>( "viewPos" ) );
				m.Shader.SetUniform( "lightPos", store.GetGlobalProperty<Vector3>( "lightPos" ) );
			} )
		);
		materials.AddDescriptor( "unlit_panel", new MaterialDescriptor()
			.SetAttribute( "aPos", MeshDescriptor.Position )
			.SetAttribute( "aUv", MeshDescriptor.UV )
			.SetOnBind( ( m, store ) => {
				m.Shader.SetUniform( "gProj", store.GetGlobalProperty<Matrix4>( "gProj" ) );
			} )
		);
		deps.Cache( materials );
		deps.Cache( textures );
		return base.CreateChildDependencies( deps );
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );
		host?.DrawThread.Scheduler.Add( () => {
			DisposeScheduler.Execute();
		} );
	}

	public RenderPiepline? GetRenderPiepline ()
		=> singleDrawNode;

	RenderPiepline? singleDrawNode;
	protected sealed override DrawNode CreateDrawNode ()
		=> singleDrawNode ??= CreateRenderPipeline();

	protected virtual RenderPiepline CreateRenderPipeline ()
		=> new BasicRenderPiepline( this );
}
