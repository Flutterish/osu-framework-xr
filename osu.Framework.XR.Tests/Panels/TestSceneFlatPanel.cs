using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Panels;

public partial class TestSceneFlatPanel : BasicTestScene {
	public TestSceneFlatPanel () {
		TestFlatPanel panel;
		Scene.Add( panel = new TestFlatPanel {
			ContentAutoSizeAxes = Axes.Both
		} );

		Box rotatingBox;
		panel.Content.Add( new Box { Size = new( 500 ), Colour = Color4.Green } );
		panel.Content.Add( new Box { Size = new( 250 ), Colour = Color4.Red } );
		panel.Content.Add( new Box { Position = new( 250 ), Size = new( 250 ), Colour = Color4.Blue } );
		panel.Content.Add( rotatingBox = new Box { Origin = Anchor.Centre, Anchor = Anchor.Centre, Size = new( 100 ) } );
		rotatingBox.OnUpdate += d => {
			d.Rotation = (float)d.Time.Current / 10;
		};

		AddSliderStep( "Scale X", 0.5f, 2f, 1, v => panel.ScaleX = v );
		AddSliderStep( "Scale Y", 0.5f, 2f, 1, v => panel.ScaleY = v );

		AddSliderStep( "Box Left", -2, 2f, -1, v => panel.BoundingBox = panel.BoundingBox with { Left = v } );
		AddSliderStep( "Box Right", -2, 2f, 1, v => panel.BoundingBox = panel.BoundingBox with { Right = v } );
		AddSliderStep( "Box Top", -2, 2f, 1, v => panel.BoundingBox = panel.BoundingBox with { Top = v } );
		AddSliderStep( "Box Bottom", -2, 2f, -1, v => panel.BoundingBox = panel.BoundingBox with { Bottom = v } );
	}

	partial class TestFlatPanel : FlatPanel {
		Box2 boundingBox = new() { Bottom = -1, Top = 1, Left = -1, Right = 1 };
		new public Box2 BoundingBox {
			get => boundingBox;
			set {
				boundingBox = value;
				InvalidateMesh();
			}
		}

		protected override Box2 RegenrateMeshWithBounds () {
			Mesh.AddQuad( new Quad3 {
				TL = new Vector3( boundingBox.Left, boundingBox.Top, 0 ),
				TR = new Vector3( boundingBox.Right, boundingBox.Top, 0 ),
				BL = new Vector3( boundingBox.Left, boundingBox.Bottom, 0 ),
				BR = new Vector3( boundingBox.Right, boundingBox.Bottom, 0 )
			} );
			return boundingBox;
		}
	}
}
