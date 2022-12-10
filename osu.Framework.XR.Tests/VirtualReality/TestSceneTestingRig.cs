namespace osu.Framework.XR.Tests.VirtualReality;

public partial class TestSceneTestingRig : BasicTestScene {
	public TestSceneTestingRig () {
		Add( new Testing.VirtualReality.TestingRig( Scene ) );
	}
}
