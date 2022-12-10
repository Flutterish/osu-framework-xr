using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public partial class TestSceneTestingRig : BasicTestScene {
	public TestSceneTestingRig () {
		Add( new Testing.VirtualReality.TestingRig( Scene ) );
	}
}
