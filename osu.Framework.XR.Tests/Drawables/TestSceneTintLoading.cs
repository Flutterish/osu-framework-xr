using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Drawables;

public partial class TestSceneTintLoading : BasicTestScene {
	public TestSceneTintLoading () {
		AddStep( "Run Test", () => {
			Scene.Clear();
			Scene.Add( new FlashContainer {
				Child = new BasicModel { Mesh = BasicMesh.UnitCornerCube, Tint = Color4.Red }
			} );
		} );
	}

	partial class FlashContainer : Container3D {
		[BackgroundDependencyLoader]
		private void load () {
			foreach ( var i in Children ) {
				i.FlashColour( Colour4.Blue, 1000 );
			}
		}
	}
}
