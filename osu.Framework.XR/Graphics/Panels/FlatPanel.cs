using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Materials;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics.Panels;

/// <summary>
/// A <see cref="Panel"/>, which renders directly into 3D space without a frame buffer
/// </summary>
public partial class FlatPanel : Panel {
	protected sealed override void RegenrateMesh () {
		base.RegenrateMesh();
	}

	protected override Material GetDefaultMaterial ( MaterialStore materials )
		=> materials.GetNew( MaterialNames.Blit );

	protected override FlatPanelDrawNode CreatePanelDrawNode ( int index )
		=> new( this, index );

	protected class FlatPanelDrawNode : PanelDrawNode {
		public FlatPanelDrawNode ( FlatPanel source, int index ) : base( source, index ) { }

		new protected FlatPanel Source => (FlatPanel)base.Source;

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( VAO.Bind() ) {
				LinkAttributeArray( Mesh, Material );
			}

			Material.BindUniforms();
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
			SourceDrawNode?.Draw( renderer );
			renderer.PopProjectionMatrix();

			renderer.PopStencilInfo();

			VAO.Bind();
			Material.BindUniforms();
			Material.Shader.SetUniform( "tint", Color4.Transparent );
			Material.Shader.SetUniform( "mMatrix", Matrix );

			renderer.PushStencilInfo( new( true, testValue: 0 ) );
			Mesh.Draw();
			renderer.PopStencilInfo();
			renderer.PopDepthInfo();
		}
	}
}
