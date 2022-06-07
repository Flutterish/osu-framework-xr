using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.XR.Collections;

namespace osu.Framework.XR.Graphics.Rendering;

partial class Scene {
	public class RenderPiepline : DrawNode, ICompositeDrawNode {
		Dictionary<Enum, HashList<Drawable3D>> renderStages = new();
		protected IEnumerable<Enum> RenderStages => renderStages.Keys;
		protected ReadOnlySpan<Drawable3D> GetRenderStage ( Enum stage ) => renderStages[stage].AsSpan();
		protected bool TryGetRenderStage ( Enum stage, out ReadOnlySpan<Drawable3D> drawables ) {
			if ( renderStages.TryGetValue( stage, out var hashList ) ) {
				drawables = hashList.AsSpan();
				return true;
			}
			drawables = default;
			return false;
		}

		protected virtual void AddDrawable ( Drawable3D drawable, Enum stage ) {
			if ( !renderStages.TryGetValue( stage, out var set ) )
				renderStages.Add( stage, set = new() );

			set.Add( drawable );
		}
		protected virtual void RemoveDrawable ( Drawable3D drawable, Enum stage ) {
			if ( !renderStages.TryGetValue( stage, out var set ) )
				renderStages.Add( stage, set = new() );

			set.Remove( drawable );
		}

		new protected Scene Source => (Scene)base.Source;

		FrameBuffer frameBuffer;
		public RenderPiepline ( Scene source ) : base( source ) {
			frameBuffer = new( new[] { osuTK.Graphics.ES30.RenderbufferInternalFormat.DepthComponent32f } );
		}

		Quad screenSpaceDrawQuad;
		Vector2 size;
		IShader blitShader = null!;
		Matrix4 projectionMatrix;
		public override void ApplyState () {
			base.ApplyState();

			screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
			blitShader = Source.blitShader;
			size = Source.DrawSize;
			projectionMatrix = Source.Camera.GetProjectionMatrix( size.X, size.Y );
		}

		public sealed override void Draw ( Action<TexturedVertex2D> vertexAction ) {
			lock ( Source.uploadMutex ) {
				while ( Source.uploadableQueue.TryDequeue( out var data ) ) {
					if ( data.added )
						AddDrawable( data.drawable, data.stage );
					else
						RemoveDrawable( data.drawable, data.stage );
				}
			}

			UploadScheduler.Execute();
			DisposeScheduler.Execute();

			frameBuffer.Size = size;
			Draw( frameBuffer, projectionMatrix );

			blitShader.Bind();
			frameBuffer.Texture.Bind();
			// the texture is upside-down because o!f says Y+ is down but we and GL say Y+ is up
			DrawQuad( frameBuffer.Texture, new Quad(
				screenSpaceDrawQuad.BottomLeft,
				screenSpaceDrawQuad.BottomRight,
				screenSpaceDrawQuad.TopLeft,
				screenSpaceDrawQuad.TopRight
			), DrawColourInfo.Colour );
		}

		public void Draw ( FrameBuffer frameBuffer, Matrix4 projectionMatrix ) {
			frameBuffer.Bind();
			GLWrapper.PushMaskingInfo( new MaskingInfo {
				ScreenSpaceAABB = new( 0, 0, (int)frameBuffer.Size.X, (int)frameBuffer.Size.Y ),
				MaskingRect = new( 0, 0, size.X, size.Y ),
				ToMaskingSpace = Matrix3.Identity,
				BlendRange = 1,
				AlphaExponent = 1
			}, true );
			GLWrapper.PushViewport( new( 0, 0, (int)frameBuffer.Size.X, (int)frameBuffer.Size.Y ) );
			GLWrapper.PushDepthInfo( new( function: osuTK.Graphics.ES30.DepthFunction.Less ) );
			GLWrapper.PushScissorState( false );
			GLWrapper.Clear( new( depth: 1 ) );

			using ( var read = Source.tripleBuffer.Get( UsageType.Read ) ) {
				Draw( read.Index, projectionMatrix );
			}

			DrawNode3D.SwitchTo2DContext();
			GLWrapper.PopScissorState();
			GLWrapper.PopDepthInfo();
			GLWrapper.PopViewport();
			GLWrapper.PopMaskingInfo();
			frameBuffer.Unbind();
		}

		protected virtual void Draw ( int subtreeIndex, Matrix4 projectionMatrix ) {
			var ctx = new BasicDrawContext( projectionMatrix );

			foreach ( var stage in RenderStages ) {
				foreach ( var i in GetRenderStage( stage ) ) {
					i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw( ctx );
				}
			}
		}

		protected override void Dispose ( bool isDisposing ) {
			frameBuffer.Dispose();
			base.Dispose( isDisposing );
		}

		public List<DrawNode>? Children { get; set; }
		public bool AddChildDrawNodes => false;
	}
}

