using osu.Framework.Allocation;
using osu.Framework.IO.Stores;
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
		var materials = new MaterialStore( new NamespacedResourceStore<byte[]>( parent.Get<Game>().Resources, "Resources/Shaders" ) );
		deps.Cache( materials );
		return base.CreateChildDependencies( deps );
	}
}
