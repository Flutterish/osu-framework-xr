using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Testing;
using osu.Framework.XR.Graphics.Meshes;
using osuTK;
using osu.Framework.XR.Maths;
using osu.Framework.Graphics;

namespace osu.Framework.XR.Tests.Maths;

public partial class TestSceneGlobalProperties : BasicTestScene {
	TransformIndicator parentTransform;
	TransformIndicator childTransform;
	TransformIndicator resultTransform;

	BasicModel box;
	BasicModel resultBox;
	Container3D container;

	public TestSceneGlobalProperties () {
		Add( parentTransform = new( Scene ) { Kind = Kind.Control } );
		Add( childTransform = new( Scene ) { Kind = Kind.Control } );
		Add( resultTransform = new( Scene ) { Kind = Kind.Result } );

		Scene.Add( resultBox = new BasicModel { Mesh = BasicMesh.UnitCube, Colour = Colour4.Red, Alpha = 0.5f, Scale = new(1.1f) } );
		Scene.Add( container = new() { Child = box = new BasicModel { Mesh = BasicMesh.UnitCube } } );

		parentTransform.RotationBindable.Value = Vector3.UnitX.LookRotation();
		childTransform.PositionBindable.Value = Vector3.UnitX;

		(parentTransform.PositionBindable, parentTransform.RotationBindable).BindValuesChanged( ( pos, rot ) => {
			(container.Position, container.Rotation) = (pos, rot);
		}, true );
		(childTransform.PositionBindable, childTransform.RotationBindable).BindValuesChanged( ( pos, rot ) => {
			(box.Position, box.Rotation) = (pos, rot);
		}, true );

		AddSliderStep( "Parent scale X", 0.1f, 2f, 1f, s => container.ScaleX = s );
		AddSliderStep( "Parent scale Y", 0.1f, 2f, 1f, s => container.ScaleY = s );
		AddSliderStep( "Parent scale Z", 0.1f, 2f, 1f, s => container.ScaleZ = s );
		AddSliderStep( "Child scale X", 0.1f, 2f, 1f, s => box.ScaleX = s );
		AddSliderStep( "Child scale Y", 0.1f, 2f, 1f, s => box.ScaleY = s );
		AddSliderStep( "Child scale Z", 0.1f, 2f, 1f, s => box.ScaleZ = s );
	}

	protected override void Update () {
		base.Update();

		resultTransform.PositionBindable.Value = resultBox.Position = box.GlobalPosition;
		resultTransform.RotationBindable.Value = resultBox.Rotation = box.GlobalRotation;
		resultBox.Scale = 1.1f * box.GlobalScale;
	}
}
