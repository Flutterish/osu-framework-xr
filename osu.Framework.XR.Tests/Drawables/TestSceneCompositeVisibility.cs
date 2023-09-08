using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Meshes;

namespace osu.Framework.XR.Tests.Drawables;

public partial class TestSceneCompositeVisibility : BasicTestScene {
	BasicModel box;
	Container3D container;
	public TestSceneCompositeVisibility () {
		Scene.Add( container = new() { Child = box = new BasicModel { Mesh = BasicMesh.UnitCornerCube } } );

		AddToggleStep( "Toggle Container Visibility", v => container.IsVisible = v );
		AddToggleStep( "Toggle Box Visibility", v => box.IsVisible = v );
	}
}
