using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.XR.Collections;
using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Shaders;
using osu.Framework.XR.Parsing.Wavefront;
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
		onVisibilityChangedDelegate = onVisibilityChanged;
		AddInternal( Root );
		Root.SubscribeSubtreeModified( 
			added: ( d, p, _ ) => {
				if ( d is IUnrenderable )
					return;

				d.RenderStageChanged += onRenderStageChangedDelegate;
				d.VisibilityChanged += onVisibilityChangedDelegate;

				if ( !d.IsRendered )
					return;

				drawables.Add( d );
				drawableQueue.Enqueue( (d, true, d.RenderStage) );
			}, 
			removed: ( d, p, _ ) => {
				if ( d is IUnrenderable )
					return;

				d.RenderStageChanged -= onRenderStageChangedDelegate;
				d.VisibilityChanged -= onVisibilityChangedDelegate;

				if ( !d.IsRendered )
					return;

				drawables.Remove( d );
				drawableQueue.Enqueue( (d, false, d.RenderStage) );
			} 
		);
	}

	// cached delegate to avoid allocs
	Action<Drawable3D, bool> onVisibilityChangedDelegate;
	private void onVisibilityChanged ( Drawable3D drawable, bool isRendered ) {
		if ( isRendered ) {
			drawables.Add( drawable );
			drawableQueue.Enqueue( (drawable, true, drawable.RenderStage) );
		}
		else {
			drawables.Remove( drawable );
			drawableQueue.Enqueue( (drawable, false, drawable.RenderStage) );
		}
	}

	// cached delegate to avoid allocs
	Drawable3D.RenderStageChangedHandler onRenderStageChangedDelegate;
	private void onRenderStageChanged ( Drawable3D drawable, Enum from, Enum to ) {
		if ( !drawable.IsRendered )
			return;

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
	private void load ( ShaderManager shaders, IRenderer renderer ) {
		if ( !renderer.GetType().Name.Contains( "GL" ) ) { // TODO currently we only work with OpenGL, we need to support other targets too
			throw new InvalidOperationException( $"osu.Framework.XR currently only supports OpenGL, but the renderer type was {renderer.GetType().ReadableName()}" );
		}

		blitShader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
	}
	IShader blitShader = null!;

	public MaterialStore MaterialStore { get; private set; } = null!;
	public MeshStore MeshStore { get; private set; } = null!;
	/// <summary>
	/// Creates a material store
	/// </summary>
	protected virtual MaterialStore CreateMaterialStore ( IReadOnlyDependencyContainer dependencies ) {
		return new MaterialStore( new ResourceStore<byte[]>( new[] {
			new NamespacedResourceStore<byte[]>( dependencies.Get<Game>().Resources, "Shaders" ),
			new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof(Scene).Assembly ), "Resources/Shaders" )
		} ) );
	}

	/// <summary>
	/// Adds material descriptors to the material store
	/// </summary>
	protected virtual void CreateMaterialDescriptors ( MaterialStore materials, IReadOnlyDependencyContainer dependencies ) {
		var renderer = dependencies.Get<IRenderer>();

		materials.AddDescriptor( MaterialNames.Unlit, new MaterialDescriptor()
			.SetAttribute( UnlitMaterial.Position, MeshDescriptor.Position )
			.SetAttribute( UnlitMaterial.UV, MeshDescriptor.UV )
			.SetUniform( UnlitMaterial.Texture, renderer.WhitePixel )
			.SetUniform( UnlitMaterial.TextureRect, renderer.WhitePixel.GetTextureRect() )
			.SetUniform( UnlitMaterial.Tint, Color4.White )
			.SetUniform( UnlitMaterial.UseGamma, true )
			.SetOnBind( ( m, store ) => {
				m.Shader.SetUniform( Shader.StandardGlobalProjectionName, store.GetGlobalProperty<Matrix4>( Shader.StandardGlobalProjectionName ) );
			} )
		);
		materials.AddDescriptor( MaterialNames.Blit, new MaterialDescriptor( materials.GetDescriptor( MaterialNames.Unlit ) ) );
		materials.SetGlobalProperty( "lightPos", Vector3.Zero );
		materials.AddDescriptor( "lit", new MaterialDescriptor( materials.GetDescriptor( MaterialNames.Unlit ) )
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
		materials.AddDescriptor( MaterialNames.UnlitPanel, new MaterialDescriptor()
			.SetAttribute( UnlitPanelMaterial.Position, MeshDescriptor.Position )
			.SetAttribute( UnlitPanelMaterial.UV, MeshDescriptor.UV )
			.SetUniform( UnlitPanelMaterial.Tint, Color4.White )
			.SetOnBind( ( m, store ) => {
				m.Shader.SetUniform( Shader.StandardGlobalProjectionName, store.GetGlobalProperty<Matrix4>( Shader.StandardGlobalProjectionName ) );
			} )
		);
	}

	/// <summary>
	/// Creates a mesh store
	/// </summary>
	protected virtual MeshStore CreateMeshStore ( IReadOnlyDependencyContainer dependencies ) {
		return new MeshStore();
	}

	/// <summary>
	/// Adds loader stores to the mesh store
	/// </summary>
	protected virtual void CreateMeshStoreSources ( MeshStore meshes, IReadOnlyDependencyContainer dependencies ) {
		var resources = new NamespacedResourceStore<byte[]>( dependencies.Get<Game>().Resources, "Meshes" );
		meshes.AddStore( new SingleObjMeshStore( resources ) );
		meshes.AddStore( new ObjMeshCollectionStore( resources ) );
	}

	GameHost? host;
	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );
		host = parent.Get<GameHost>();

		deps.Cache( MeshStore = CreateMeshStore( deps ) );
		CreateMeshStoreSources( MeshStore, deps );
		deps.Cache( MaterialStore = CreateMaterialStore( deps ) );
		CreateMaterialDescriptors( MaterialStore, deps );

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
