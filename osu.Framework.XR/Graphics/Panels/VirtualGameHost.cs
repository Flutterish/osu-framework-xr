using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Handlers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Threading;
using System.Collections.Immutable;
using System.Reflection;

namespace osu.Framework.XR.Graphics.Panels;

/// <summary>
/// A fake game host which allows to separate a game from the main game host and manipulate its properties.
/// By default everything falls though, except the ability to quit/suspend the process and IPC. It is also not possible to <see cref="GameHost.Run(Game)"/>.
/// <example>
/// <code>
/// protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
///		var deps = new DependencyContainer( parent );
///		VirtualGameHost = new( parent.Get&lt;GameHost&gt;() );
/// 	deps.CacheAs&lt;GameHost&gt;( VirtualGameHost );
/// 	return base.CreateChildDependencies( deps );
/// }
/// [BackgroundDependencyLoader]
/// private void load () {
/// 	var game = new SomeGame();
/// 	game.SetHost( VirtualGameHost );
/// 	AddInternal( game );
/// }
/// </code>
/// </example>
/// </summary>
public class VirtualGameHost : GameHost {
	public readonly GameHost Parent;
	public VirtualGameHost ( GameHost parent, string? name = null, HostOptions? options = null ) : base( name ?? parent.Name, options ?? parent.Options ) {
		Parent = parent;
		Storage = parent.Storage;
		CacheStorage = parent.CacheStorage;
		AllowBenchmarkUnlimitedFrames = parent.AllowBenchmarkUnlimitedFrames;

		AvailableInputHandlers = parent.AvailableInputHandlers;
		Renderer = parent.Renderer;
		Window = parent.Window;
		DrawThread = parent.DrawThread;
		UpdateThread = parent.UpdateThread;
		AudioThread = parent.AudioThread;
		InputThread = parent.InputThread;
		ThreadRunner = (ThreadRunner)threadRunner.GetValue( parent )!;
	}

	new public Bindable<bool> IsActive => (Bindable<bool>)base.IsActive;
	/// <inheritdoc cref="GameHost.Storage"/>
	new public Storage Storage {
		get => base.Storage;
		init => base.Storage = value;
	}
	/// <inheritdoc cref="GameHost.CacheStorage"/>
	new public Storage CacheStorage {
		get => base.CacheStorage;
		init => base.CacheStorage = value;
	}
	/// <inheritdoc cref="GameHost.AvailableInputHandlers"/>
	new public ImmutableArray<InputHandler> AvailableInputHandlers {
		get => base.AvailableInputHandlers;
		init => typeof( GameHost ).GetProperty( nameof( AvailableInputHandlers ) )!.GetSetMethod( true )!.Invoke( this, new object[] { value } );
	}
	/// <inheritdoc cref="GameHost.Renderer"/>
	new public IRenderer Renderer {
		get => base.Renderer;
		init => typeof( GameHost ).GetProperty( nameof( Renderer ) )!.GetSetMethod( true )!.Invoke( this, new object[] { value } );
	}
	/// <inheritdoc cref="GameHost.Window"/>
	new public IWindow Window {
		get => base.Window;
		init => typeof( GameHost ).GetProperty( nameof( Window ) )!.GetSetMethod( true )!.Invoke( this, new object[] { value } );
	}
	/// <inheritdoc cref="GameHost.DrawThread"/>
	new public DrawThread DrawThread {
		get => base.DrawThread;
		init => typeof( GameHost ).GetProperty( nameof( DrawThread ) )!.GetSetMethod( true )!.Invoke( this, new object[] { value } );
	}
	/// <inheritdoc cref="GameHost.UpdateThread"/>
	new public GameThread UpdateThread {
		get => base.UpdateThread;
		init => typeof( GameHost ).GetProperty( nameof( UpdateThread ) )!.GetSetMethod( true )!.Invoke( this, new object[] { value } );
	}
	/// <inheritdoc cref="GameHost.InputThread"/>
	new public InputThread InputThread {
		get => base.InputThread;
		init => typeof( GameHost ).GetProperty( nameof( InputThread ) )!.GetSetMethod( true )!.Invoke( this, new object[] { value } );
	}
	/// <inheritdoc cref="GameHost.AudioThread"/>
	new public AudioThread AudioThread {
		get => base.AudioThread;
		init => typeof( GameHost ).GetProperty( nameof( AudioThread ) )!.GetSetMethod( true )!.Invoke( this, new object[] { value } );
	}
	static readonly FieldInfo threadRunner = typeof( GameHost ).GetField( "threadRunner", BindingFlags.NonPublic | BindingFlags.Instance )!;
	/// <inheritdoc cref="ThreadRunner"/>
	public ThreadRunner ThreadRunner {
		get => (ThreadRunner)threadRunner.GetValue( this )!;
		init => threadRunner.SetValue( this, value );
	}

	public override bool CanExit => false;
	public override bool CanSuspendToBackground => false;
	public override bool CapsLockEnabled => Parent.CapsLockEnabled;
	public override string InitialFileSelectorPath => Parent.InitialFileSelectorPath;
	public override bool IsPrimaryInstance { 
		get => false;
		protected set => base.IsPrimaryInstance = value;
	}
	public override bool OnScreenKeyboardOverlapsGameWindow => false;
	public override IEnumerable<KeyBinding> PlatformKeyBindings => Parent.PlatformKeyBindings;
	public override IEnumerable<string> UserStoragePaths => Parent.UserStoragePaths;

	protected override ReadableKeyCombinationProvider CreateReadableKeyCombinationProvider () {
		return Parent.Dependencies.Get<ReadableKeyCombinationProvider>();
	}
	protected override TextInputSource CreateTextInput () {
		return Parent.Dependencies.Get<TextInputSource>();
	}
	public override IResourceStore<TextureUpload> CreateTextureLoaderStore ( IResourceStore<byte[]> underlyingStore ) {
		return Parent.CreateTextureLoaderStore( underlyingStore );
	}
	public override VideoDecoder CreateVideoDecoder ( Stream stream ) {
		return Parent.CreateVideoDecoder( stream );
	}
	public override Clipboard GetClipboard () {
		return Parent.GetClipboard();
	}
	public override Storage? GetStorage ( string path ) {
		return Parent.GetStorage( path );
	}
	public override bool OpenFileExternally ( string filename ) {
		return Parent.OpenFileExternally( filename );
	}
	public override void OpenUrlExternally ( string url ) {
		Parent.OpenUrlExternally( url );
	}
	protected override void PerformExit ( bool immediately ) { }
	public override bool PresentFileExternally ( string filename ) {
		return Parent.PresentFileExternally( filename );
	}
	public override Task SendMessageAsync ( IpcMessage message ) {
		return Parent.SendMessageAsync( message );
	}
	public override bool SuspendToBackground () {
		return false;
	}

	protected override IEnumerable<InputHandler> CreateAvailableInputHandlers ()
		=> throw new NotSupportedException( "Nested game host does not support this operation" );
	protected override void SetupConfig ( IDictionary<FrameworkSetting, object> defaultOverrides )
		=> throw new NotSupportedException( "Nested game host does not support this operation" );
	protected override void SetupForRun ()
		=> throw new NotSupportedException( "Nested game host does not support this operation" );
	protected override IRenderer CreateRenderer ()
		=> throw new NotSupportedException( "Nested game host does not support this operation" );
	protected override ThreadRunner CreateThreadRunner ( InputThread mainThread )
		=> throw new NotSupportedException( "Nested game host does not support this operation" );
	protected override IWindow CreateWindow ( GraphicsSurfaceType preferredSurface )
		=> throw new NotSupportedException( "Nested game host does not support this operation" );
	protected override void DrawFrame ()
		=> throw new NotSupportedException( "Nested game host does not support this operation" );
	protected override void UpdateFrame ()
		=> throw new NotSupportedException( "Nested game host does not support this operation" );
	protected override void Swap ()
		=> throw new NotSupportedException( "Nested game host does not support this operation" );
}