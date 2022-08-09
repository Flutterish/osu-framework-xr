using osu.Framework.Allocation;
using osu.Framework.Graphics.Rendering;
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
		var susieTexture = textures.Get( "susie", WrapMode.ClampToEdge, WrapMode.ClampToEdge );

		susieCubeMaterial.CreateUpload( m => {
			m.Set( "tex", susieTexture );
			m.Set( "subImage", susieTexture.GetTextureRect() );
		} ).Enqueue();

		return new TestingRenderPiepline( this );
	}

	class TestingRenderPiepline : BasicRenderPiepline {
		new protected TestingScene Source => (TestingScene)base.Source;
		public TestingRenderPiepline ( TestingScene source ) : base( source ) { }

		protected override void Draw ( IRenderer renderer, int subtreeIndex, Matrix4 projectionMatrix ) {
			foreach ( var stage in RenderStages ) {
				if ( stage is TestingRenderStage.SusieCubeBatch ) {
					DrawCubes( renderer, subtreeIndex, projectionMatrix, GetRenderStage( stage ) );
				}
				else {
					foreach ( var i in GetRenderStage( stage ) ) {
						i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw( renderer );
					}
				}
			}
		}

		void DrawCubes ( IRenderer renderer, int subtreeIndex, Matrix4 projectionMatrix, ReadOnlySpan<Drawable3D> cubes ) {
			var VAO = BatchedSusieCube.VAO;
			var mesh = BatchedSusieCube.Mesh;
			var material = Source.susieCubeMaterial;
			if ( VAO.Handle == 0 ) {
				VAO.Bind();

				DrawNode3D.LinkAttributeArray( mesh, material );
			}
			else VAO.Bind();

			material.Bind();
			material.Shader.SetUniform( "gProj", ref projectionMatrix );
			foreach ( var i in cubes ) {
				i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw( renderer, material.Shader );
				mesh.Draw();
			}
		}
	}
}

public enum TestingRenderStage {
	Default,
	SusieCubeBatch
}