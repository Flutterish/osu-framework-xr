using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Materials;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics.Panels;

/// <summary>
/// A <see cref="Panel"/>, which renders directly into 3D space without a frame buffer
/// </summary>
public class FlatPanel : Panel {
	protected sealed override void RegenrateMesh () {
		base.RegenrateMesh();
	}

	protected override Material GetDefaultMaterial ( MaterialStore materials )
		=> materials.GetNew( "blit" );

	protected override FlatPanelDrawNode CreatePanelDrawNode ()
		=> new( this );

	protected class FlatPanelDrawNode : PanelDrawNode {
		public FlatPanelDrawNode ( FlatPanel source ) : base( source ) { }

		new protected FlatPanel Source => (FlatPanel)base.Source;

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( VAO.Bind() ) {
				LinkAttributeArray( Mesh, Material );
			}

			Material.Bind();
			Material.Shader.SetUniform( "tint", Color4.Transparent );
			Material.Shader.SetUniform( "mMatrix", Matrix );

			renderer.PushStencilInfo( new( true ) );
			Mesh.Draw();
			renderer.PopStencilInfo();

			SwitchTo2DContext( renderer );
			renderer.PushStencilInfo( new( true, BufferTestFunction.Equal, passed: StencilOperation.Keep ) );
			renderer.PushDepthInfo( new( true, false, BufferTestFunction.Always ) );

			renderer.PushProjectionMatrix( Matrix4.CreateTranslation( 0, 0, -1 ) 
				* Matrix4.CreateScale( 2 / Size.X, -2 / Size.Y, 1 )
				* Matrix4.CreateTranslation( -1, 1, 0 )
				* Matrix
				* renderer.ProjectionMatrix 
			);
			using ( var buffer = Source.TripleBuffer.GetForRead() ) {
				var node = Source.ContentDrawNodes[buffer.Index];
				node?.Draw( renderer );
			}
			renderer.PopProjectionMatrix();

			renderer.PopStencilInfo();

			VAO.Bind();
			Material.Bind();
			Material.Shader.SetUniform( "tint", Color4.Transparent );
			Material.Shader.SetUniform( "mMatrix", Matrix );

			renderer.PushStencilInfo( new( true, testValue: 0 ) );
			Mesh.Draw();
			renderer.PopStencilInfo();
			renderer.PopDepthInfo();
		}
	}
}
