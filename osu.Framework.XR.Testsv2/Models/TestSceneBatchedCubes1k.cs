﻿using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Testing;

namespace osu.Framework.XR.Tests.Models;
public class TestSceneBatchedCubes1k : TestScene3D {
	public TestSceneBatchedCubes1k () {
		for ( int i = 0; i < 1000; i++ ) {
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
