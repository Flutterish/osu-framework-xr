using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Rendering;

namespace osu.Framework.XR.Tests.Models;
public partial class TestSceneBatchedCubes10k : BasicTestScene {
	public TestSceneBatchedCubes10k () {
		for ( int i = 0; i < 10000; i++ ) {
			Scene.Add( new BatchedSusieCube {
				X = RNG.NextSingle( -5, 5 ),
				Y = RNG.NextSingle( -5, 5 ),
				Z = RNG.NextSingle( -5, 5 )
			} );
		}

		Scene.Camera.Z = -10;
	}

	protected override Scene CreateScene ()
		=> new TestingScene();
}
