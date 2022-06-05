using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Rendering;

namespace osu.Framework.XR.Tests.Models;

public class TestSceneBatchDrawableCubes10k : BasicTestScene {
	public TestSceneBatchDrawableCubes10k () {
		var batch = new SusieCubeBatch();
		Scene.Add( batch );

		for ( int i = 0; i < 10000; i++ ) {
			batch.Add( new BatchDrawableSusieCube {
				X = RNG.NextSingle( -5, 5 ),
				Y = RNG.NextSingle( -5, 5 ),
				Z = RNG.NextSingle( -5, 5 )
			} );
		}

		Scene.Camera.Z = -10;
	}
}
