using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics.Panels;
using osuTK.Graphics;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Components;
using osu.Framework.XR.Testing;
using osuTK;

namespace osu.Framework.XR.Tests.Panels;

public class TestSceneMultiplePanels : TestScene3D {
	PhysicsSystem physics = new();
	PanelInteractionSystem interactionSystem;

	public TestSceneMultiplePanels () {
		physics.AddSubtree( Scene.Root );
		Add( interactionSystem = new PanelInteractionSystem( Scene, physics ) { RelativeSizeAxes = Axes.Both } );

		Scene.Add( createPanel( new( RNG.NextSingle( -5, 5 ), RNG.NextSingle( -5, 5 ), RNG.NextSingle( -5, 5 ) ) ) );
		Scene.Add( createPanel( new( RNG.NextSingle( -5, 5 ), RNG.NextSingle( -5, 5 ), RNG.NextSingle( -5, 5 ) ) ) );
		Scene.Add( createPanel( new( RNG.NextSingle( -5, 5 ), RNG.NextSingle( -5, 5 ), RNG.NextSingle( -5, 5 ) ) ) );
		Scene.Add( createPanel( new( RNG.NextSingle( -5, 5 ), RNG.NextSingle( -5, 5 ), RNG.NextSingle( -5, 5 ) ) ) );

		AddToggleStep( "Use Touch", v => {
			interactionSystem.UseTouch = v;
		} );
	}

	Panel createPanel ( Vector3 pos ) {
		Panel p = new();
		p.ContentSize = new( 200 );
		p.Content.Add( new Box { Colour = Color4.Gray, RelativeSizeAxes = Axes.Both } );
		p.Content.Add( new BasicTextBox() { Width = 100, Height = 20 } );
		p.Content.Add( new CursorContainer { RelativeSizeAxes = Axes.Both } );
		p.Position = pos;
		return p;
	}
}
