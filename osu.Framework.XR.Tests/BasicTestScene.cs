using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Testing;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests;

public class BasicTestScene : TestScene3D {
	public BasicTestScene () {
		Scene.Camera.Z = -10;

		Scene.Add( new BasicModel() {
			Mesh = BasicMesh.UnitCube,
			Colour = Color4.Red,
			Scale = new( 10, 0.03f, 0.03f ),
			Origin = new( -1, 0, 0 )
		} );
		Scene.Add( new BasicModel() {
			Mesh = BasicMesh.UnitCube,
			Colour = Color4.Green,
			Scale = new( 0.03f, 10, 0.03f ),
			Origin = new( 0, -1, 0 )
		} );
		Scene.Add( new BasicModel() {
			Mesh = BasicMesh.UnitCube,
			Colour = Color4.Blue,
			Scale = new( 0.03f, 0.03f, 10 ),
			Origin = new( 0, 0, -1 )
		} );
	}

	protected override Scene CreateScene () {
		return new TestResourcesScene();
	}

	class TestResourcesScene : Scene {
		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
			var dependencies = new DependencyContainer( parent );

			var Resources = new ResourceStore<byte[]>();
			Resources.AddStore( new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof( TestResourcesScene ).Assembly ), @"Resources" ) );
			var host = parent.Get<GameHost>();

			var Textures = new TextureStore(
				host.Renderer,
				parent.Get<GameHost>().CreateTextureLoaderStore( new NamespacedResourceStore<byte[]>( Resources, "Textures" ) ),
				useAtlas: true,
				TextureFilteringMode.Nearest,
				manualMipmaps: false,
				scaleAdjust: 1
			);

			dependencies.Cache( Textures );

			return base.CreateChildDependencies( dependencies );
		}
	}
}
