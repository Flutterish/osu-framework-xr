using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;

namespace osu.Framework.XR.Tests.Drawables;

public partial class TestSceneVisibility : BasicTestScene {
	BasicModel box;
	public TestSceneVisibility () {
		Scene.Add( box = new BasicModel { Mesh = BasicMesh.UnitCube } );

		AddToggleStep( "Toggle Visibility", v => box.IsVisible = v );
	}
}
