using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Testing;

namespace osu.Framework.XR.Tests;

public class SampleTestScene : TestScene3D {
	public SampleTestScene () {
		for ( int i = 0; i < 1000; i++ ) {
			Scene.Add( new Model {
				X = RNG.NextSingle( -5, 5 ),
				Y = RNG.NextSingle( -5, 5 ),
				Z = RNG.NextSingle( -5, 5 )
			} );
		}
	}
}
