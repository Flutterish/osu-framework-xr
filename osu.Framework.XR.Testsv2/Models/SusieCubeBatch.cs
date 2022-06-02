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
		material = materials.GetNew( "unlit" );
		var texture = textures.Get( "susie", Framework.Graphics.OpenGL.Textures.WrapMode.ClampToEdge, Framework.Graphics.OpenGL.Textures.WrapMode.ClampToEdge );

		material.CreateUpload( m => {
			m.Set( "tex", texture );
			m.Set( "subImage", texture.GetTextureRect() );
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

			mesh.ElementBuffer!.Bind();
			mesh.VertexBuffers[0].Link( material.Shader, new int[] { material.Shader.GetAttrib( "aPos" ), material.Shader.GetAttrib( "aUv" ) } );
		}

		protected override void DrawBatch ( object? ctx = null ) {
			var projectionMatrix = ( (BasicDrawContext)ctx! ).ProjectionMatrix;
			var mesh = BatchedSusieCube.Mesh;
			var material = Source.material;
			var mMatrix = material.Shader.GetUniform<Matrix4>( "mMatrix" );
			var tint = material.Shader.GetUniform<Color4>( "tint" );

			material.Bind();
			material.Shader.SetUniform( "gProj", ref projectionMatrix );
			foreach ( var i in Children ) {
				mMatrix.UpdateValue( ref i.matrix );
				tint.UpdateValue( ref i.color );
				mesh.Draw();
			}
		}
	}
}
