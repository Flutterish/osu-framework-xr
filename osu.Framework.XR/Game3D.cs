using osu.Framework.Allocation;
using osu.Framework.IO.Stores;
using osu.Framework.XR.Materials;
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.XR {
	public abstract class Game3D : Game {
		[NotNull, MaybeNull]
		private DependencyContainer dependencies;
		[NotNull, MaybeNull]
		private MaterialManager MaterialManager;

		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) =>
			dependencies = new DependencyContainer( base.CreateChildDependencies( parent ) );

		[BackgroundDependencyLoader]
		private void load () {
			var resources = new ResourceStore<byte[]>();
			resources.AddStore( new NamespacedResourceStore<byte[]>( Resources, @"Shaders" ) );
			resources.AddStore( new NamespacedResourceStore<byte[]>( Resources, @"Shaders/Materials" ) );
			MaterialManager = new MaterialManager( resources );
			dependencies.CacheAs( MaterialManager );
		}
	}
}
