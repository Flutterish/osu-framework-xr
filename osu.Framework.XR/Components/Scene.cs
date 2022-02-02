using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Projection;
using osuTK;
using System;
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.XR.Components {
	/// <summary>
	/// A scene containing 3D objects.
	/// </summary>
	public class Scene : Container {
		public Scene () {
			Add( new SceneDrawer( this ) );

			base.Add( Root );
		}

		public override void Add ( Drawable drawable ) {
			if ( drawable is Drawable3D xro )
				Root.Add( xro );
			else
				base.Add( drawable );
		}

		public bool RenderToScreen { 
			get => RenderToScreenBindable.Value; 
			set => RenderToScreenBindable.Value = value; 
		}
		public readonly BindableBool RenderToScreenBindable = new( false );
		public readonly Container3D Root = new Container3D();
		[MaybeNull] [NotNull]
		public Camera Camera { get; init; }

		public static implicit operator CompositeDrawable3D ( Scene scene )
			=> scene.Root;

		[MaybeNull, NotNull]
		private IShader TextureShader;
		private DepthFrameBuffer depthBuffer = new();

		[BackgroundDependencyLoader]
		private void load ( ShaderManager shaders ) {
			TextureShader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
		}

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
			depthBuffer.Dispose();
		}

		private class SceneDrawer : Drawable { // for whatever reason o!f doesnt use the XrScenes draw node ( prolly bc its a container )
			public Scene Scene;

			public SceneDrawer ( Scene scene ) {
				RelativeSizeAxes = Axes.Both;
				Scene = scene;
			}

			protected override DrawNode CreateDrawNode ()
				=> new XrSceneDrawNode( Scene );
		}

		private class XrSceneDrawNode : DrawNode {
			new private Scene Source;
			public XrSceneDrawNode ( Scene source ) : base( source ) {
				Source = source;
			}

			Vector2 size;
			Quad quad;
			[MaybeNull, NotNull]
			IShader textureShader;
			public override void ApplyState () {
				base.ApplyState();
				quad = Source.ScreenSpaceDrawQuad;
				size = quad.Size;
				quad = new Quad( quad.BottomLeft, quad.BottomRight, quad.TopLeft, quad.TopRight );
				textureShader = Source.TextureShader;
			}

			public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
				if ( !Source.RenderToScreen ) return;

				if ( Source.depthBuffer.Size != size ) Source.depthBuffer.Size = size;

				if ( Source.Camera is not null ) {
					Source.Camera.Render( Source.depthBuffer );

					base.Draw( vertexAction );
					if ( Source.depthBuffer.Texture?.Bind() == true ) {
						textureShader.Bind();
						DrawQuad( Source.depthBuffer.Texture, quad, DrawColourInfo.Colour );
						textureShader.Unbind();
					}
				}
			}
		}
	}
}
