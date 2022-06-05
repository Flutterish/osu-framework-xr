using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Rendering;
using osuTK;
using System;

namespace osu.Framework.XR.Tests.Models;

public class TestingScene : Scene {
	Material susieCubeMaterial = null!;
	protected override RenderPiepline CreateRenderPipeline () {
		var materials = Dependencies.Get<MaterialStore>();
		var textures = Dependencies.Get<TextureStore>();

		susieCubeMaterial = materials.GetNew( "unlit" );
		var susieTexture = textures.Get( "susie", Framework.Graphics.OpenGL.Textures.WrapMode.ClampToEdge, Framework.Graphics.OpenGL.Textures.WrapMode.ClampToEdge );

		susieCubeMaterial.CreateUpload( m => {
			m.Set( "tex", susieTexture );
			m.Set( "subImage", susieTexture.GetTextureRect() );
		} ).Enqueue();

		return new TestingRenderPiepline( this );
	}

	class TestingRenderPiepline : RenderPiepline {
		new protected TestingScene Source => (TestingScene)base.Source;
		public TestingRenderPiepline ( TestingScene source ) : base( source ) { }

		protected override void Draw ( int subtreeIndex, Matrix4 projectionMatrix ) {
			var ctx = new BasicDrawContext( projectionMatrix );

			foreach ( var stage in RenderStages ) {
				if ( stage is TestingRenderStage.SusieCubeBatch ) {
					DrawCubes( subtreeIndex, projectionMatrix, GetRenderStage( stage ) );
				}
				else {
					foreach ( var i in GetRenderStage( stage ) ) {
						i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw( ctx );
					}
				}
			}
		}

		void DrawCubes ( int subtreeIndex, Matrix4 projectionMatrix, ReadOnlySpan<Drawable3D> cubes ) {
			var VAO = BatchedSusieCube.VAO;
			var mesh = BatchedSusieCube.Mesh;
			var material = Source.susieCubeMaterial;
			if ( VAO.Handle == 0 ) {
				VAO.Bind();

				mesh.ElementBuffer!.Bind();
				mesh.VertexBuffers[0].Link( material.Shader, stackalloc int[] { material.Shader.GetAttrib( "aPos" ), material.Shader.GetAttrib( "aUv" ) } );
			}
			else VAO.Bind();

			material.Bind();
			material.Shader.SetUniform( "gProj", ref projectionMatrix );
			foreach ( var i in cubes ) {
				i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw( material.Shader );
				mesh.Draw();
			}
		}
	}
}

public enum TestingRenderStage {
	Default,
	SusieCubeBatch
}