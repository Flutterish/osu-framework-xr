using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Panels;

public partial class TestSceneNested : TestScenePanelInput {
	public TestSceneNested () {
		Panel.Content.Clear();

		Panel.Content.Add( new Box { Size = new( 500 ), Colour = Color4.Transparent } );
		Panel.Content.Add( new TestScenePanelInput() );
	}
}
