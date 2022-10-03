using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Materials;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Models;

public class SusieCubeBatch : BatchDrawable<BatchDrawableSusieCube, BatchDrawableSusieCube.BatchedSusieCubeDrawNode> {
	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials, TextureStore textures ) {
		material = materials.GetNew( MaterialNames.Unlit );
		var texture = textures.Get( "susie", WrapMode.ClampToEdge, WrapMode.ClampToEdge );

		material.CreateUpload( m => {
			m.SetUniform( "tex", texture );
			m.SetUniform( "subImage", texture.GetTextureRect() );
		} ).Enqueue();
	}
	Material material = null!;

	protected override BatchDrawNode CreateDrawNode3D ( int subtreeIndex )
		=> new SusieCubeBatchDrawNode( this, subtreeIndex );

	class SusieCubeBatchDrawNode : BatchDrawNode {
		new SusieCubeBatch Source => (SusieCubeBatch)base.Source;
		public SusieCubeBatchDrawNode ( SusieCubeBatch source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override void Initialize () {
			var mesh = BatchedSusieCube.Mesh;
			var material = Source.material;

			LinkAttributeArray( mesh, material );
		}

		protected override void DrawBatch ( object? ctx = null ) {
			var mesh = BatchedSusieCube.Mesh;
			var material = Source.material;
			var mMatrix = material.Shader.GetUniform<Matrix4>( "mMatrix" );
			var tint = material.Shader.GetUniform<Color4>( "tint" );

			material.BindUniforms();
			foreach ( var i in Children ) {
				mMatrix.UpdateValue( ref i.matrix );
				tint.UpdateValue( ref i.color );
				mesh.Draw();
			}
		}
	}
}
