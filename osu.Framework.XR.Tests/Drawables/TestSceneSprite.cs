using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics;
using System;

namespace osu.Framework.XR.Tests.Drawables;

public partial class TestSceneSprite : BasicTestScene {
	Sprite3D sprite;
	BasicModel fillbox;
	BasicModel fillboxBorder;
	public TestSceneSprite () {
		Scene.Add( sprite = new() { /*EulerY = MathF.PI*/ } );
		Scene.Add( fillbox = new() { Mesh = BasicMesh.UnitQuad, Tint = Color4.Blue, Z = 0.01f } );
		Scene.Add( fillboxBorder = new() { Mesh = BasicMesh.UnitQuad, Tint = Color4.Red, Z = 0.015f } );

		Scene.OnLoadComplete += _ => {
			sprite.Texture = Scene.Dependencies.Get<TextureStore>().Get( "susie" );
		};

		AddToggleStep( "Face Camera", v => sprite.FaceCamera = v );
		AddSliderStep( "Aspect Ratio", 0.1f, 10f, 1f, v => sprite.FillAspectRatio = v );
		AddSliderStep( "FillBox X", 0.1f, 10f, 1f, v => sprite.Width = v );
		AddSliderStep( "FillBox Y", 0.1f, 10f, 1f, v => sprite.Height = v );

		AddSliderStep( "FillBox Anchor X", -1f, 1f, 0, v => sprite.FillBoxAnchorPosition = sprite.FillBoxAnchorPosition with { X = v } );
		AddSliderStep( "FillBox Anchor Y", -1f, 1f, 0, v => sprite.FillBoxAnchorPosition = sprite.FillBoxAnchorPosition with { Y = v } );
		AddSliderStep( "Anchor X", -1f, 1f, 0, v => sprite.AnchorPosition = sprite.AnchorPosition with { X = v } );
		AddSliderStep( "Anchor Y", -1f, 1f, 0, v => sprite.AnchorPosition = sprite.AnchorPosition with { Y = v } );
		AddSliderStep( "Origin X", -1f, 1f, 0, v => sprite.OriginPosition = sprite.OriginPosition with { X = v } );
		AddSliderStep( "Origin Y", -1f, 1f, 0, v => sprite.OriginPosition = sprite.OriginPosition with { Y = v } );
		AddLabel( "FillMode" );
		foreach ( var i in Enum.GetValues<FillMode>() ) {
			AddStep( $"{i}", () => sprite.FillMode = i );
		}
	}

	protected override void Update () {
		base.Update();
		fillboxBorder.Origin = fillbox.Origin = new osuTK.Vector3( sprite.FillBoxAnchorPosition.X, sprite.FillBoxAnchorPosition.Y, 0 ) / 2;
		fillbox.Scale = new( sprite.Width, sprite.Height, 1 );
		fillboxBorder.Scale = fillbox.Scale + new osuTK.Vector3( 0.02f, 0.02f, 0 );
	}
}
