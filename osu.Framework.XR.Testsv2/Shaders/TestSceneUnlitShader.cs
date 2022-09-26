using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osuTK;
using System;
using System.Collections.Generic;

namespace osu.Framework.XR.Tests.Shaders;

public class TestSceneUnlitShader : BasicTestScene {
	public TestSceneUnlitShader () {
		var rng = new Random( 457576346 );
		for ( int i = 0;i < 5; i++ ) {
			BasicModel model;
			Scene.Add( model = new BasicModel { Mesh = BasicMesh.UnitCube } );
			var col = new Colour4( rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1 );
			AddSliderStep( $"Cube {i+1} alpha", 0, 1, rng.NextSingle( 0.2f, 0.8f ), a => model.Colour = col.Opacity( a ) );

			model.Position = new Vector3( rng.NextSingle( -1, 1 ), rng.NextSingle( -1, 1 ), rng.NextSingle( -1, 1 ) ) * 5;
		}
	}
}
