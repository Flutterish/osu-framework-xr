using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.XR.Collections;
using osu.Framework.XR.Graphics.Materials;

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
			GLWrapper.PushViewport( new( 0, 0, (int)frameBuffer.Size.X, (int)frameBuffer.Size.Y ) );
			GLWrapper.PushScissorState( false );
			GLWrapper.PushDepthInfo( new( function: osuTK.Graphics.ES30.DepthFunction.Less ) );
			GLWrapper.Clear( new( depth: 1 ) );

			using ( var read = Source.tripleBuffer.Get( UsageType.Read ) ) {
				Draw( read.Index, projectionMatrix );
			}

			Shaders.Shader.Unbind();
			Material.Unbind();
			GL.BindVertexArray( 0 );
			GLWrapper.PopDepthInfo();
			GLWrapper.PopScissorState();
			GLWrapper.PopViewport();
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

		public List<DrawNode>? Children { get; set; }
		public bool AddChildDrawNodes => false;
	}
}

