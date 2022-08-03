using OpenVR.NET;
using OpenVR.NET.Devices;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;
using System.Diagnostics.CodeAnalysis;

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
		new protected VrCompositor Source => (VrCompositor)base.Source;
		public VrCompositorDrawNode ( VrCompositor source ) : base( source ) { }

		FrameBuffer? left;
		FrameBuffer? right;
		public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
			var context = Source.VR?.UpdateDraw();
			if ( context is null || Source.VR!.Headset is not Headset headset )
				return;

			if ( ( Source.ActivePlayer?.Root as Drawable )?.Parent is not Scene scene || scene.GetRenderPiepline() is not Scene.RenderPiepline pipeline )
				return;

			left ??= new FrameBuffer( new[] { osuTK.Graphics.ES30.RenderbufferInternalFormat.DepthComponent32f } );
			right ??= new FrameBuffer( new[] { osuTK.Graphics.ES30.RenderbufferInternalFormat.DepthComponent32f } );

			var size = new Vector2( context.Resolution.X, context.Resolution.Y );
			left.Size = size;
			right.Size = size;

			Matrix4 headTransform = Matrix4.CreateFromQuaternion( headset.RenderRotation.ToOsuTk() ) 
				* Matrix4.CreateTranslation( headset.RenderPosition.ToOsuTk() );

			var lEye = context.GetEyeToHeadMatrix( Valve.VR.EVREye.Eye_Left ).ToOsuTk().Inverted();
			var lProj = context.GetProjectionMatrix( Valve.VR.EVREye.Eye_Left, 0.01f, 1000f ).ToOsuTk();
			var rEye = context.GetEyeToHeadMatrix( Valve.VR.EVREye.Eye_Right ).ToOsuTk().Inverted();
			var rProj = context.GetProjectionMatrix( Valve.VR.EVREye.Eye_Right, 0.01f, 1000f ).ToOsuTk();

			pipeline.Draw( left, headTransform.Inverted() * lEye * Matrix4.CreateScale( 1, 1, -1 ) * lProj );
			pipeline.Draw( right, headTransform.Inverted() * rEye * Matrix4.CreateScale( 1, 1, -1 ) * rProj );

			context.SubmitFrame( Valve.VR.EVREye.Eye_Left, new() { eType = Valve.VR.ETextureType.OpenGL, handle = (IntPtr)left.Texture.TextureId } );
			context.SubmitFrame( Valve.VR.EVREye.Eye_Right, new() { eType = Valve.VR.ETextureType.OpenGL, handle = (IntPtr)right.Texture.TextureId } );
		}

		protected override void Dispose ( bool isDisposing ) {
			left?.Dispose();
			right?.Dispose();

			base.Dispose( isDisposing );
		}
	}
}
