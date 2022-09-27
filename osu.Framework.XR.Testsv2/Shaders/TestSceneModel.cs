using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;

namespace osu.Framework.XR.Tests.Shaders;

public class TestSceneModel : BasicTestScene {
	Model model;

	public TestSceneModel () {
		Scene.Add( model = new() );
		model.Mesh = BasicMesh.UnitCube;

		AddSliderStep( "Alpha", 0, 1, 1f, v => model.Alpha = v );
		AddStep( "Random Color", () => model.Colour = new( RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), model.Alpha ) );
		AddStep( "White", () => model.Colour = new( 1, 1, 1, model.Alpha ) );
		AddLabel( "Material" );
		foreach ( var mat in new[] { "unlit", "blit" } ) {
			AddStep( mat, () => model.Material = Scene.MaterialStore.GetNew( mat ) );
		}
	}
}
