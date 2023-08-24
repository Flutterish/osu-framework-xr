using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Shaders;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Rendering;

partial class Scene {
	public abstract class RenderPiepline : DrawNode, ICompositeDrawNode {
		protected MaterialStore MaterialStore => Source.MaterialStore;
		protected abstract void AddDrawable ( Drawable3D drawable, Enum stage );
		protected abstract void RemoveDrawable ( Drawable3D drawable, Enum stage );

		new protected Scene Source => (Scene)base.Source;

		IFrameBuffer? frameBuffer;
		public RenderPiepline ( Scene source ) : base( source ) { }

		protected virtual Vector2 GetFrameBufferSize () {
			return Source.ScreenSpaceDrawQuad.Size;
		}

		Quad screenSpaceDrawQuad;
		Vector2 size;
		IShader blitShader = null!;
		Matrix4 projectionMatrix;
		public override void ApplyState () {
			base.ApplyState();

			screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
			blitShader = Source.blitShader;
			size = GetFrameBufferSize();
			projectionMatrix = Source.Camera.GetProjectionMatrix( size.X, size.Y );
		}

		public sealed override void Draw ( IRenderer renderer ) {
			lock ( Source.uploadMutex ) {
				while ( Source.uploadableQueue.TryDequeue( out var data ) ) {
					if ( data.added )
						AddDrawable( data.drawable, data.stage );
					else
						RemoveDrawable( data.drawable, data.stage );
				}
			}

			DrawNode3D.SwitchTo3DContext( renderer );
			UploadScheduler.Execute( renderer );
			DisposeScheduler.Execute();

			frameBuffer ??= renderer.CreateFrameBuffer( new[] { RenderBufferFormat.D32S8 } );
			frameBuffer.Size = size;
			Draw( renderer, frameBuffer, projectionMatrix );

			base.Draw( renderer );
			blitShader.Bind();
			frameBuffer.Texture.Bind();
			// the texture is upside-down because o!f says Y+ is down but we and GL say Y+ is up
			renderer.DrawQuad( frameBuffer.Texture, new Quad(
				screenSpaceDrawQuad.BottomLeft,
				screenSpaceDrawQuad.BottomRight,
				screenSpaceDrawQuad.TopLeft,
				screenSpaceDrawQuad.TopRight
			), DrawColourInfo.Colour );
			blitShader.Unbind();
		}

		public void Draw ( IRenderer renderer, IFrameBuffer frameBuffer, Matrix4 projectionMatrix, bool clearFramebuffer = true ) {
			frameBuffer.Bind();
			renderer.PushMaskingInfo( new MaskingInfo {
				ScreenSpaceAABB = new( 0, 0, (int)frameBuffer.Size.X, (int)frameBuffer.Size.Y ),
				MaskingRect = new( 0, 0, size.X, size.Y ),
				ToMaskingSpace = Matrix3.Identity,
				BlendRange = 1,
				AlphaExponent = 1
			}, true );
			renderer.PushViewport( new( 0, 0, (int)frameBuffer.Size.X, (int)frameBuffer.Size.Y ) );
			renderer.PushDepthInfo( new( function: BufferTestFunction.LessThan ) );
			renderer.PushScissorState( false );
			if ( clearFramebuffer )
				renderer.Clear( new( depth: 1 ) );

			renderer.PushProjectionMatrix( projectionMatrix );
			MaterialStore.SetGlobalProperty( Shader.StandardGlobalProjectionName, projectionMatrix );
			MaterialStore.SetGlobalProperty( "viewPos", projectionMatrix.ExtractCameraPosition() );
			using ( var read = Source.tripleBuffer.GetForRead() ) {
				Draw( renderer, read.Index, projectionMatrix );
			}
			renderer.PopProjectionMatrix();

			DrawNode3D.SwitchTo2DContext( renderer );
			renderer.PopScissorState();
			renderer.PopDepthInfo();
			renderer.PopViewport();
			renderer.PopMaskingInfo();
			frameBuffer.Unbind();
		}

		protected abstract void Draw ( IRenderer renderer, int subtreeIndex, Matrix4 projectionMatrix );

		protected override void Dispose ( bool isDisposing ) {
			frameBuffer?.Dispose();
			base.Dispose( isDisposing );
		}

		public List<DrawNode>? Children { get; set; }
		public bool AddChildDrawNodes => false;
	}
}

