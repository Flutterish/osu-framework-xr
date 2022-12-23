using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Shaders;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics.Panels;

/// <summary>
/// A <see cref="Panel"/>, which renders directly into 3D space without a frame buffer.
/// This uses a stencil buffer
/// </summary>
public partial class FlatPanel : Panel {
	protected sealed override void RegenrateMesh () {
		boundinbgBox = RegenrateMeshWithBounds();
	}

	Box2 boundinbgBox;
	/// <summary>
	/// Regenrate mesh after it's been invalidated though <see cref="Panel.InvalidateMesh()"/>.
	/// </summary>
	/// <returns>
	/// The bounding box of the mesh
	/// </returns>
	protected virtual Box2 RegenrateMeshWithBounds () {
		base.RegenrateMesh();
		return new() { Bottom = -1, Top = 1, Left = -1, Right = 1 };
	}

	protected override Material GetDefaultMaterial ( MaterialStore materials )
		=> materials.GetNew( MaterialNames.Blit );

	protected override FlatPanelDrawNode CreatePanelDrawNode ( int index )
		=> new( this, index );

	protected class FlatPanelDrawNode : PanelDrawNode {
		public FlatPanelDrawNode ( FlatPanel source, int index ) : base( source, index ) { }

		new protected FlatPanel Source => (FlatPanel)base.Source;

		Box2 boundinbgBox;
		protected override void UpdateState () {
			base.UpdateState();
			boundinbgBox = Source.boundinbgBox;
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( VAO.Bind() ) {
				LinkAttributeArray( Mesh, Material );
			}

			Material.BindUniforms();
			Material.Shader.SetUniform( BlitMaterial.Tint, Color4.Transparent );
			Material.Shader.SetUniform( Shader.StandardLocalMatrixName, Matrix );

			renderer.PushStencilInfo( new( true ) );
			Mesh.Draw();
			renderer.PopStencilInfo();

			SwitchTo2DContext( renderer );
			renderer.PushStencilInfo( new( true, BufferTestFunction.Equal, passed: StencilOperation.Keep ) );
			renderer.PushDepthInfo( new( true, false, BufferTestFunction.Always ) );

			renderer.PushProjectionMatrix( Matrix4.CreateTranslation( 0, 0, -1 ) 
				* Matrix4.CreateScale( boundinbgBox.Width / Size.X, -boundinbgBox.Height / Size.Y, 1 )
				* Matrix4.CreateTranslation( boundinbgBox.Left, boundinbgBox.Top, 0 )
				* Matrix
				* renderer.ProjectionMatrix 
			);
			SourceDrawNode?.Draw( renderer );
			renderer.PopProjectionMatrix();

			renderer.PopStencilInfo();

			VAO.Bind();
			Material.BindUniforms();
			Material.Shader.SetUniform( BlitMaterial.Tint, Color4.Transparent );
			Material.Shader.SetUniform( Shader.StandardLocalMatrixName, Matrix );

			renderer.PushStencilInfo( new( true, testValue: 0 ) );
			Mesh.Draw();
			renderer.PopStencilInfo();
			renderer.PopDepthInfo();
		}
	}
}
