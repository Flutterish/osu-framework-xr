﻿using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Shaders;
using osuTK.Graphics;
using System.Reflection;

namespace osu.Framework.XR.Graphics.Panels;

public partial class Panel {
	// yuck, internal
	static MethodInfo generateDrawNodeSubtree = typeof( Drawable ).GetMethod( "GenerateDrawNodeSubtree", BindingFlags.Instance | BindingFlags.NonPublic )!;
	public static DrawNode? GenerateDrawNodeSubtree ( Drawable drawable, ulong frameId, int treeIndex, bool forceNewDrawNode )
		=> generateDrawNodeSubtree.Invoke( drawable, new object[] { frameId, treeIndex, forceNewDrawNode } ) as DrawNode;

	ulong frameId = 0;

	protected sealed override DrawNode3D? CreateDrawNode3D ( int index )
		=> CreatePanelDrawNode( index );
	protected virtual PanelDrawNode CreatePanelDrawNode ( int index )
		=> new( this, index );

	protected IFrameBuffer? FrameBuffer; // shared data
	AttributeArray VAO = new();
	ulong lastRenderedFrame;
	protected class PanelDrawNode : DrawNode3D {
		protected DrawNode? SourceDrawNode { get; private set; }

		new protected Panel Source => (Panel)base.Source;
		protected readonly AttributeArray VAO;
		protected readonly BasicMesh Mesh;
		protected readonly Material Material;
		protected IFrameBuffer? FrameBuffer {
			get => Source.FrameBuffer;
			set => Source.FrameBuffer = value;
		}
		protected Matrix4 Matrix;
		protected Vector2 Size;
		protected readonly int SubtreeIndex;
		ulong meshId;
		public PanelDrawNode ( Panel source, int index ) : base( source ) {
			VAO = source.VAO;
			Mesh = source.Mesh;
			Material = source.Material;
			SubtreeIndex = index;
		}

		protected override void UpdateState () {
			Matrix = Source.Matrix;
			Size = Source.ContentDrawSize;

			Material.UpdateProperties( SubtreeIndex );
			SourceDrawNode = GenerateDrawNodeSubtree( Source.Content, Source.frameId++, SubtreeIndex, false );
			meshId = Source.meshId;
		}

		protected virtual void BeforeBlit ( IRenderer renderer, object? ctx = null ) {
			renderer.SetBlend( BlendingParameters.Mixture );
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( Source.lastRenderedFrame >= renderer.FrameIndex ) {
				goto blit;
			}

			Source.lastRenderedFrame = renderer.FrameIndex;
			SwitchTo2DContext( renderer );
			Vector2I size = new( (int)Size.X, (int)Size.Y );
			RectangleI rect = new( 0, 0, size.X, size.Y );
			FrameBuffer ??= renderer.CreateFrameBuffer( new[] { RenderBufferFormat.D32S8 } );
			FrameBuffer.Size = size;
			FrameBuffer.Bind();
			renderer.PushMaskingInfo( new MaskingInfo {
				ScreenSpaceAABB = rect,
				MaskingRect = rect,
				ToMaskingSpace = Matrix3.Identity,
				BlendRange = 1,
				AlphaExponent = 1,
				CornerExponent = 2.5f
			}, true );
			renderer.PushViewport( rect );
			renderer.PushOrtho( rect );
			renderer.PushDepthInfo( new() );
			renderer.PushStencilInfo( StencilInfo.Default );
			renderer.PushScissorState( true );
			renderer.PushScissor( rect );
			renderer.PushScissorOffset( Vector2I.Zero );
			renderer.Clear( new( colour: Color4.Transparent ) );

			SourceDrawNode?.Draw( renderer );

			renderer.PopScissorOffset();
			renderer.PopScissor();
			renderer.PopScissorState();
			renderer.PopStencilInfo();
			renderer.PopDepthInfo();
			renderer.PopOrtho();
			renderer.PopViewport();
			renderer.PopMaskingInfo();
			FrameBuffer.Unbind();

			SwitchTo3DContext( renderer );

			blit:
			if ( VAO.Bind() || meshId > Source.linkedMeshId ) {
				LinkAttributeArray( Mesh, Material );
				Source.linkedMeshId = meshId;
			}

			Material.Bind( SubtreeIndex );
			Material.Shader.SetUniform( Material.StandardTextureName, FrameBuffer!.Texture );
			Material.Shader.SetUniform( Shader.StandardLocalMatrixName, ref Matrix );
			BeforeBlit( renderer, ctx );
			Mesh.Draw();
		}
	}
}
