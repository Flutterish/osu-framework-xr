namespace osu.Framework.XR.Tests.VirtualReality;

public class TestSceneTestingRig : BasicTestScene {
	public TestSceneTestingRig () {
		Add( new Testing.VirtualReality.TestingRig( Scene ) );
	}
}
