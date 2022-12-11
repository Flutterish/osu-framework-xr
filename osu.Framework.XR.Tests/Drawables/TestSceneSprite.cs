using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics;
using System;
using System.Linq;

namespace osu.Framework.XR.Tests.Drawables;

public partial class TestSceneSprite : BasicTestScene {
	Sprite3D sprite;
	BasicModel fillbox;
	BasicModel fillboxBorder;
	public TestSceneSprite () {
		Scene.Add( sprite = new() { EulerY = MathF.PI } );
		Scene.Add( fillbox = new() { Mesh = BasicMesh.UnitQuad, Tint = Color4.Blue, Z = 0.01f } );
		Scene.Add( fillboxBorder = new() { Mesh = BasicMesh.UnitQuad, Tint = Color4.Red, Z = 0.015f } );

		Scene.OnLoadComplete += _ => {
			sprite.Texture = Scene.Dependencies.Get<TextureStore>().Get( "susie" );
		};

		AddToggleStep( "Face Camera", v => sprite.FaceCamera = v );
		AddSliderStep( "Aspect Ratio", 0.1f, 10f, 1f, v => sprite.FillAspectRatio = v );
		AddSliderStep( "FillBox X", 0.1f, 10f, 1f, v => sprite.Width = v );
		AddSliderStep( "FillBox Y", 0.1f, 10f, 1f, v => sprite.Height = v );
		AddLabel( "FillBox Anchor" );
		foreach ( var i in Enum.GetValues<Anchor>().Where( x => x.ToString().Length != 2 && x != Anchor.Custom ) ) {
			AddStep( $"{i}", () => sprite.FillBoxAnchor = i );
		}
		AddLabel( "Origin" );
		foreach ( var i in Enum.GetValues<Anchor>().Where( x => x.ToString().Length != 2 && x != Anchor.Custom ) ) {
			AddStep( $"{i}", () => sprite.Origin = i );
		}
		AddLabel( "FillMode" );
		foreach ( var i in Enum.GetValues<FillMode>() ) {
			AddStep( $"{i}", () => sprite.FillMode = i );
		}
	}

	protected override void Update () {
		base.Update();
		fillbox.Scale = new( sprite.Width / 2, sprite.Height / 2, 1 );
		if ( sprite.FillBoxAnchor.HasFlag( Anchor.x0 ) )
			fillbox.OriginX = -1;
		else if ( sprite.FillBoxAnchor.HasFlag( Anchor.x2 ) )
			fillbox.OriginX = 1;
		else
			fillbox.OriginX = 0;

		if ( sprite.FillBoxAnchor.HasFlag( Anchor.y0 ) )
			fillbox.OriginY = 1;
		else if ( sprite.FillBoxAnchor.HasFlag( Anchor.y2 ) )
			fillbox.OriginY = -1;
		else
			fillbox.OriginY = 0;

		fillboxBorder.Origin = fillbox.Origin;
		fillboxBorder.Scale = fillbox.Scale + new osuTK.Vector3( 0.02f, 0.02f, 0 );
	}
}
