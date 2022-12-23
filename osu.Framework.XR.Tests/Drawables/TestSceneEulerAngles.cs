using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using System;

namespace osu.Framework.XR.Tests.Drawables;

public partial class TestSceneEulerAngles : BasicTestScene {
	public TestSceneEulerAngles () {
		SpriteText text;
		Add( text = new() {
			Position = new(5)
		} );

		Model model;
		Scene.Add( model = new() {
			Mesh = BasicMesh.UnitQuad
		} );

		void updateDisplay () {
			text.Text = $"X: {model.EulerX:N2} Y: {model.EulerY:N2} Z: {model.EulerZ:N2}";
		}
		AddSliderStep( "Euler X", -MathF.Tau, MathF.Tau, 0, v => {
			model.EulerX = v;
			updateDisplay();
		} );
		AddSliderStep( "Euler Y", -MathF.Tau, MathF.Tau, 0, v => {
			model.EulerY = v;
			updateDisplay();
		} );
		AddSliderStep( "Euler Z", -MathF.Tau, MathF.Tau, 0, v => {
			model.EulerZ = v;
			updateDisplay();
		} );
		updateDisplay();
	}
}
