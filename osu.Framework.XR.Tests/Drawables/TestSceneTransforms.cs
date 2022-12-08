using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics;
using osu.Framework.Graphics;

namespace osu.Framework.XR.Tests.Drawables;

public partial class TestSceneTransfoms : BasicTestScene {
	BasicModel box;
	public TestSceneTransfoms () {
		Scene.Add( box = new BasicModel { Mesh = BasicMesh.UnitCube } );

		AddStep( "Flash colour", () => box.FlashColour( Colour4.Red, 400 ) );
	}
}
