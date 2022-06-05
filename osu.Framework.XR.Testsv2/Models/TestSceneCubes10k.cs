using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Rendering;

namespace osu.Framework.XR.Tests.Models;

public class TestSceneCubes10k : BasicTestScene {
	public TestSceneCubes10k () {
		for ( int i = 0; i < 10000; i++ ) {
			Scene.Add( new SusieCube {
				X = RNG.NextSingle( -5, 5 ),
				Y = RNG.NextSingle( -5, 5 ),
				Z = RNG.NextSingle( -5, 5 )
			} );
		}

		Scene.Camera.Z = -10;
	}
}
