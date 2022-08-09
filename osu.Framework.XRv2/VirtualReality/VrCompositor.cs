using OpenVR.NET;
using OpenVR.NET.Devices;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace osu.Framework.XR.VirtualReality;

/// <summary>
/// Manages what gets shown to the vr user, allows to select the controlled <see cref="VrPlayer"/>,
/// and calls the lifetime events of VR.
/// Only one <see cref="VrCompositor"/> can be active at a time - if there is a parent
/// <see cref="VrCompositor"/>, this one will be deactivated
/// </summary>
[Cached]
public class VrCompositor : Drawable {
	[Resolved(canBeNull: true)]
	new protected VrCompositor? Parent { get; private set; }

	List<VrPlayer> players = new();
	List<VrCompositor> children = new();
	public void RegisterPlayer ( VrPlayer player ) {
		players.Add( player );
		ActivePlayer ??= player;
	}

	public VrPlayer? ActivePlayer { get; private set; }
	VR? VR;
	[MemberNotNullWhen(true, nameof(VR))]
	[MemberNotNullWhen(false, nameof(Parent))]
	protected bool IsRootCompositor => true;
	protected override void LoadComplete () {
		if ( IsRootCompositor ) {
			VR = new();
			var ok = VR.TryStart();

		}
		else {
			Parent.children.Add( this );
		}
	}

	protected override void Update () {
		if ( IsRootCompositor ) {
			VR.Update();
			VR.UpdateInput();
		}
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			VR?.Exit();
			VR = null;
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
			var type = native.GetType();
			// assume TextureGL right now
			_getGlTextureId ??= type.GetField( "textureId", BindingFlags.Instance | BindingFlags.NonPublic )!;
			return (IntPtr)(int)_getGlTextureId.GetValue( texture )!;
		}

		new protected VrCompositor Source => (VrCompositor)base.Source;
		public VrCompositorDrawNode ( VrCompositor source ) : base( source ) { }

		IFrameBuffer? left;
		IFrameBuffer? right;
		public override void Draw ( IRenderer renderer ) {
			var context = Source.VR?.UpdateDraw();
			if ( context is null || Source.VR!.Headset is not Headset headset )
				return;

			if ( ( Source.ActivePlayer?.Root as Drawable )?.Parent is not Scene scene || scene.GetRenderPiepline() is not Scene.RenderPiepline pipeline )
				return;
			

			left ??= renderer.CreateFrameBuffer( new[] { RenderBufferFormat.D16 } ); // TODO implement 32 bit depth in o!f myself (like last time)
			right ??= renderer.CreateFrameBuffer( new[] { RenderBufferFormat.D16 } );

			var size = new Vector2( context.Resolution.X, context.Resolution.Y );
			left.Size = size;
			right.Size = size;

			Matrix4 headTransform = Matrix4.CreateFromQuaternion( headset.RenderRotation.ToOsuTk() ) 
				* Matrix4.CreateTranslation( headset.RenderPosition.ToOsuTk() );

			var lEye = context.GetEyeToHeadMatrix( Valve.VR.EVREye.Eye_Left ).ToOsuTk().Inverted();
			var lProj = context.GetProjectionMatrix( Valve.VR.EVREye.Eye_Left, 0.01f, 1000f ).ToOsuTk();
			var rEye = context.GetEyeToHeadMatrix( Valve.VR.EVREye.Eye_Right ).ToOsuTk().Inverted();
			var rProj = context.GetProjectionMatrix( Valve.VR.EVREye.Eye_Right, 0.01f, 1000f ).ToOsuTk();

			pipeline.Draw( renderer, left, headTransform.Inverted() * lEye * Matrix4.CreateScale( 1, 1, -1 ) * lProj );
			pipeline.Draw( renderer, right, headTransform.Inverted() * rEye * Matrix4.CreateScale( 1, 1, -1 ) * rProj );

			context.SubmitFrame( Valve.VR.EVREye.Eye_Left, new() { eType = Valve.VR.ETextureType.OpenGL, handle = GetTexturePointer( left.Texture ) } );
			context.SubmitFrame( Valve.VR.EVREye.Eye_Right, new() { eType = Valve.VR.ETextureType.OpenGL, handle = GetTexturePointer( right.Texture ) } );
		}

		protected override void Dispose ( bool isDisposing ) {
			left?.Dispose();
			right?.Dispose();

			base.Dispose( isDisposing );
		}
	}
}
