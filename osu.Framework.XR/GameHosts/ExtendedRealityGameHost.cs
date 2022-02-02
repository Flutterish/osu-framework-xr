using OpenVR.NET;
using osu.Framework.Bindables;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osuTK;
using System;
using Valve.VR;

namespace osu.Framework.XR.GameHosts {
	public abstract class ExtendedRealityGameHost : GameHost { // TODO scale is not included in autosizing
		protected ExtendedRealityGameHost ( string gameName = "" ) : base( gameName ) {
			//IsActive.BindValueChanged( v => {
			//	if ( v.NewValue == false ) { // this makes it so osu never caps our FPS
			//		IsActive.UnbindFrom( Window.IsActive );
			//		( (Bindable<bool>)IsActive ).Value = true; // NOTE check if this might cause the load fails
			//	}
			//} );

			PlayerHeightOffsetBindable.ValueChanged += v => { // TODO a better way to apply a bindable change with a delay
				playerHeightOffsetApplyTime = SceneGraphClock.CurrentTime;
			};
		}

		public VirtualTextInput TextInput { get; } = new VirtualTextInput();

		protected override TextInputSource CreateTextInput ()
			=> TextInput;

		VirtualClipboard clipboard = new();
		public override Clipboard GetClipboard ()
			=> clipboard;

		public override void OpenFileExternally ( string filename ) {
			throw new NotImplementedException( "File dialog panel is not yet implemented" ); // TODO file dialog and browser panels
		}
		
		public override void OpenUrlExternally ( string url ) {
			throw new NotImplementedException( "Web browser panel is not yet implemented" );
		}

		XrGame? runningGame;
		DepthFrameBuffer leftEye = new();
		DepthFrameBuffer rightEye = new();
		public void Run ( XrGame game ) {
			runningGame = game;
			runningGame.OnLoadComplete += _ => VR.SetManifest( runningGame.XrManifest );

			base.Run( game );
			runningGame = null;

			VR.Exit();
		}

		public readonly BindableFloat PlayerHeightOffsetBindable = new( 0 ) { MinValue = -0.5f, MaxValue = 0.5f };
		public float PlayerHeightOffset { get; private set; } = 0;
		private double playerHeightOffsetApplyTime;
		protected override void DrawFrame () {
			base.DrawFrame();

			if ( runningGame?.IsLoaded != true ) return;
			if ( playerHeightOffsetApplyTime + 1000 < SceneGraphClock.CurrentTime ) {
				PlayerHeightOffset = PlayerHeightOffsetBindable.Value;
			}

			VR.UpdateDraw( SceneGraphClock.CurrentTime );
			if ( !VR.VrState.HasFlag( VrState.OK ) ) return;

			var size = new Vector2( VR.RenderSize.X, VR.RenderSize.Y );
			if ( leftEye.Size != size ) {
				leftEye.Size = size;
				rightEye.Size = size;
			}

			var lMatrix = VR.CVRSystem.GetProjectionMatrix( EVREye.Eye_Left, 0.01f, 1200 );
			var rMatrix = VR.CVRSystem.GetProjectionMatrix( EVREye.Eye_Right, 0.01f, 1200 );

			var leftEyeMatrix =
				new Matrix4x4(
					lMatrix.m0, lMatrix.m1, lMatrix.m2, lMatrix.m3,
					lMatrix.m4, lMatrix.m5, lMatrix.m6, lMatrix.m7,
					lMatrix.m8, lMatrix.m9, -lMatrix.m10, lMatrix.m11,
					lMatrix.m12, lMatrix.m13, -lMatrix.m14, lMatrix.m15
				);
			var rightEyeMatrix =
				new Matrix4x4(
					rMatrix.m0, rMatrix.m1, rMatrix.m2, rMatrix.m3,
					rMatrix.m4, rMatrix.m5, rMatrix.m6, rMatrix.m7,
					rMatrix.m8, rMatrix.m9, -rMatrix.m10, rMatrix.m11,
					rMatrix.m12, rMatrix.m13, -rMatrix.m14, rMatrix.m15
				);

			var el = VR.CVRSystem.GetEyeToHeadTransform( EVREye.Eye_Left );
			var er = VR.CVRSystem.GetEyeToHeadTransform( EVREye.Eye_Right );

			Matrix4x4 headToLeftEye = new Matrix4x4(
				el.m0, el.m1, el.m2, el.m3,
				el.m4, el.m5, el.m6, el.m7,
				el.m8, el.m9, el.m10, el.m11,
				0, 0, 0, 1
			);

			Matrix4x4 headToRightEye = new Matrix4x4(
				er.m0, er.m1, er.m2, er.m3,
				er.m4, er.m5, er.m6, er.m7,
				er.m8, er.m9, er.m10, er.m11,
				0, 0, 0, 1
			);

			runningGame.Player.Position = runningGame.Player.PositionOffset + new Vector3( VR.Current.Headset.Position.X, VR.Current.Headset.Position.Y + PlayerHeightOffset, VR.Current.Headset.Position.Z );

			var rot = new Quaternion( VR.Current.Headset.Rotation.X, VR.Current.Headset.Rotation.Y, VR.Current.Headset.Rotation.Z, VR.Current.Headset.Rotation.W );
			runningGame.Player.Rotation = rot.DecomposeAroundAxis( Vector3.UnitY );
			runningGame.Player.Camera.Rotation = runningGame.Player.Rotation.Inverted() * rot;

			runningGame.Player.Camera.Render( leftEye, new Drawable3D.DrawNode3D.DrawSettings { 
				WorldToCamera = (headToLeftEye * runningGame.Scene.Camera.WorldCameraMatrix).Transposed,
				CameraToClip = leftEyeMatrix.Transposed,
				GlobalCameraPos = runningGame.Player.Camera.GlobalPosition,
				GlobalCameraRot = runningGame.Player.Camera.GlobalRotation
			} );
			runningGame.Player.Camera.Render( rightEye, new Drawable3D.DrawNode3D.DrawSettings { 
				WorldToCamera = (headToRightEye * runningGame.Scene.Camera.WorldCameraMatrix).Transposed, 
				CameraToClip = rightEyeMatrix.Transposed,
				GlobalCameraPos = runningGame.Player.Camera.GlobalPosition,
				GlobalCameraRot = runningGame.Player.Camera.GlobalRotation
			} );

			Texture_t left = new Texture_t { eColorSpace = EColorSpace.Linear, eType = ETextureType.OpenGL, handle = (IntPtr)leftEye.Texture.TextureId };
			Texture_t right = new Texture_t { eColorSpace = EColorSpace.Linear, eType = ETextureType.OpenGL, handle = (IntPtr)rightEye.Texture.TextureId };
			VR.SubmitFrame( EVREye.Eye_Right, left );
			VR.SubmitFrame( EVREye.Eye_Left, right );
		}

		protected override void UpdateFrame () {
			VR.Update();
			base.UpdateFrame();
		}
	}
}
