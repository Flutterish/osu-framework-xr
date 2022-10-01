using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Panels;

public class TestSceneNested : TestScenePanelInput {
	public TestSceneNested () {
		Panel.Content.Clear();

		Panel.Content.Add( new Box { Size = new( 500 ), Colour = Color4.Transparent } );
		Panel.Content.Add( new TestScenePanelInput() );
	}
}
