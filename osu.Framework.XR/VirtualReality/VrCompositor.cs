using OpenVR.NET;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;
using osu.Framework.XR.VirtualReality.Devices;
using System.Reflection;

namespace osu.Framework.XR.VirtualReality;

/// <summary>
/// Manages what gets shown to the vr user, allows to select the controlled <see cref="VrPlayer"/>,
/// and calls the lifetime events of VR.
/// Only one <see cref="VrCompositor"/> should exist at one time. (TBD nesting)
/// </summary>
public partial class VrCompositor : Drawable {
	public IReadOnlyList<VrPlayer> Players => players;

	List<VrPlayer> players = new();
	public void RegisterPlayer ( VrPlayer player ) {
		players.Add( player );
		ActivePlayer ??= player;
	}

	VrPlayer? activePlayer;
	public VrPlayer? ActivePlayer {
		get => activePlayer;
		set {
			activePlayer = value;
		}
	}

	public VrCompositor () {
		Input = CreateInput();
		DeviceDetected += device => {
			trackedDevices.Add( device );
			device.IsEnabled.BindValueChanged( v => {
				if ( v.NewValue )
					activeDevices.Add( device );
				else
					activeDevices.Remove( device );
			}, true );
		};
	}

	bool ownVr;
	/// <summary>
	/// The underlying VR system. If you are a consumer you should use the provided 
	/// wrappers such as <see cref="VrInput"/>, <see cref="VrDevice"/> etc.
	/// </summary>
	public VR? VR { get; private set; }
	public readonly VrInput Input;
	public event Action? InitializationFailed;
	public event Action<VrCompositor>? Initialized;

	protected virtual VrInput CreateInput () => new( this );

	public event Action<VrDevice>? DeviceDetected;
	BindableList<VrDevice> trackedDevices = new();
	BindableList<VrDevice> activeDevices = new();
	public IBindableList<VrDevice> TrackedDevices => trackedDevices;
	public IBindableList<VrDevice> ActiveDevices => activeDevices;

	public void BindDeviceDetected ( Action<VrDevice> action, bool invokeOnAllImmediately = true ) {
		DeviceDetected += action;
		if ( invokeOnAllImmediately ) {
			foreach ( var i in TrackedDevices ) {
				action( i );
			}
		}
	}

	protected void OnDeviceDetected ( VrDevice device )
		=> DeviceDetected?.Invoke( device );

	protected virtual Task<VR?> InitializeVr ( IReadOnlyDependencyContainer dependencies ) {
		return Task.Run( () => {
			dependencies.TryGet<VR>( out var vr );
			vr ??= new VR();
			vr.DeviceDetected += handleDevice;
			foreach ( var i in vr.TrackedDevices ) {
				handleDevice( i );
			}
			var ok = vr.TryStart();

			return ok ? vr : null;
		} );
	}

	private void handleDevice ( OpenVR.NET.Devices.VrDevice source ) {
		VrDevice device = source switch {
			OpenVR.NET.Devices.Controller controller => new Controller( this, controller ),
			OpenVR.NET.Devices.Headset d => new Headset( this, d ),
			OpenVR.NET.Devices.DisplayRedirect d => new DisplayRedirect( this, d ),
			OpenVR.NET.Devices.Tracker d => new Tracker( this, d ),
			OpenVR.NET.Devices.TrackingReference d => new TrackingReference( this, d ),
			_ => new VrDevice( this, source )
		};

		Schedule( device => OnDeviceDetected( device ), device );
	}

	protected override void InjectDependencies ( IReadOnlyDependencyContainer dependencies ) {
		base.InjectDependencies( dependencies );

		InitializeVr( dependencies ).ContinueWith( r => {
			if ( r.Result is VR vr ) {
				ownVr = !dependencies.TryGet<VR>( out var parentVr ) || vr != parentVr;
				Schedule( () => {
					VR = vr;
					Initialized?.Invoke( this );
					Initialized = null;
				} );
			}
			else {
				Schedule( () => {
					InitializationFailed?.Invoke();
					InitializationFailed = null;
				} );
			}
		} );
	}

	protected override void Update () {
		if ( VR is null )
			return;

		VR.Update();
		VR.UpdateInput(); // TODO when input events get timestamps, we might want to move this to another thread

		if ( activePlayer is VrPlayer player && VR.Headset is OpenVR.NET.Devices.Headset headset ) {
			player.Position = player.PositionOffset + player.RotationOffset.Apply(headset.Position.ToOsuTk() with { Y = 0 });
			player.Rotation = player.RotationOffset * headset.Rotation.ToOsuTk().DecomposeAroundAxis( Vector3.UnitY );
		}
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( VR != null )
			VR.DeviceDetected -= handleDevice;

		if ( !IsDisposed ) {
			if ( ownVr )
				VR?.Exit();
			VR = null!;
		}

		base.Dispose( isDisposing );
	}

	VrCompositorDrawNode? singleDrawNode;
	protected sealed override DrawNode CreateDrawNode ()
		=> singleDrawNode ??= CreateCompositorDrawNode();
	protected virtual VrCompositorDrawNode CreateCompositorDrawNode ()
		=> new( this );

	protected class VrCompositorDrawNode : DrawNode {
		static PropertyInfo _getNativeTexture = typeof( Texture ).GetProperty( "NativeTexture", BindingFlags.Instance | BindingFlags.NonPublic )!;
		static FieldInfo? _getGlTextureId;
		static IntPtr GetTexturePointer ( Texture texture ) {
			var native = _getNativeTexture.GetValue( texture )!;
			var type = native.GetType().BaseType!;
			// assume TextureGL right now
			_getGlTextureId ??= type.GetField( "textureId", BindingFlags.Instance | BindingFlags.NonPublic )!;
			return (IntPtr)(int)_getGlTextureId.GetValue( native )!;
		}

		new protected VrCompositor Source => (VrCompositor)base.Source;
		public VrCompositorDrawNode ( VrCompositor source ) : base( source ) { }

		IFrameBuffer? left;
		IFrameBuffer? right;
		public override void Draw ( IRenderer renderer ) {
			var vr = Source.VR;
			var context = vr?.UpdateDraw();
			if ( context is null || vr!.Headset is not OpenVR.NET.Devices.Headset headset )
				return;

			if ( Source.ActivePlayer is not VrPlayer player || ( player?.Root as Drawable )?.Parent is not Scene scene || scene.GetRenderPiepline() is not Scene.RenderPiepline pipeline )
				return;

			left ??= renderer.CreateFrameBuffer( new[] { RenderBufferFormat.D32S8 } ); // TODO these should be anti-aliased
			right ??= renderer.CreateFrameBuffer( new[] { RenderBufferFormat.D32S8 } );

			var size = new Vector2( context.Resolution.X, context.Resolution.Y );
			left.Size = size;
			right.Size = size;

			Matrix4 headTransform = Matrix4.CreateFromQuaternion( player.RotationOffset * headset.RenderRotation.ToOsuTk() ) 
				* Matrix4.CreateTranslation( player.RotationOffset.Apply( headset.RenderPosition.ToOsuTk() ) + player.PositionOffset );

			var lEye = context.GetEyeToHeadMatrix( Valve.VR.EVREye.Eye_Left ).ToOsuTk().Inverted();
			var lProj = context.GetProjectionMatrix( Valve.VR.EVREye.Eye_Left, 0.01f, 1000f ).ToOsuTk();
			var rEye = context.GetEyeToHeadMatrix( Valve.VR.EVREye.Eye_Right ).ToOsuTk().Inverted();
			var rProj = context.GetProjectionMatrix( Valve.VR.EVREye.Eye_Right, 0.01f, 1000f ).ToOsuTk();

			pipeline.Draw( renderer, left, headTransform.Inverted() * lEye * Matrix4.CreateScale( 1, 1, -1 ) * lProj );
			pipeline.Draw( renderer, right, headTransform.Inverted() * rEye * Matrix4.CreateScale( 1, 1, -1 ) * rProj );

			context.SubmitFrame( Valve.VR.EVREye.Eye_Left, new() { eColorSpace = Valve.VR.EColorSpace.Linear, eType = Valve.VR.ETextureType.OpenGL, handle = GetTexturePointer( left.Texture ) } );
			context.SubmitFrame( Valve.VR.EVREye.Eye_Right, new() { eColorSpace = Valve.VR.EColorSpace.Linear, eType = Valve.VR.ETextureType.OpenGL, handle = GetTexturePointer( right.Texture ) } );
		}

		protected override void Dispose ( bool isDisposing ) {
			left?.Dispose();
			right?.Dispose();

			base.Dispose( isDisposing );
		}
	}
}
