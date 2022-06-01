using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Containers;

namespace osu.Framework.XR.Tests;

public class SampleTestScene : TestScene {
	public SampleTestScene () {
		Add( new Scene { RelativeSizeAxes = Framework.Graphics.Axes.Both } );
	}

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );
		var store = parent.Get<Game>().Resources;
		var materials = new MaterialStore( new NamespacedResourceStore<byte[]>( store, "Resources/Shaders" ) );
		var textures = new TextureStore(
			parent.Get<GameHost>().CreateTextureLoaderStore( new NamespacedResourceStore<byte[]>( store, "Resources/Textures" ) ),
			useAtlas: true,
			osuTK.Graphics.ES30.All.Nearest,
			manualMipmaps: false,
			scaleAdjust: 1
		);
		deps.Cache( materials );
		deps.Cache( textures );
		return base.CreateChildDependencies( deps );
	}
}
