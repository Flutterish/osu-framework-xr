using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Testing;

namespace osu.Framework.XR.Tests.Maths;

public class TestSceneSetGlobalProperties : BasicTestScene {
	TransformIndicator parentTransform;
	TransformIndicator childTransform;

	BasicModel box;
	Container3D container;

	public TestSceneSetGlobalProperties () {
		Add( parentTransform = new( Scene ) { Kind = Kind.Control } );
		Add( childTransform = new( Scene ) { Kind = Kind.Control } );

		Scene.Add( container = new() { Child = box = new BasicModel { Mesh = BasicMesh.UnitCube } } );

		(parentTransform.PositionBindable, parentTransform.RotationBindable).BindValuesChanged( ( pos, rot ) => {
			(container.Position, container.Rotation) = (pos, rot);
		}, true );
		(childTransform.PositionBindable, childTransform.RotationBindable).BindValuesChanged( ( pos, rot ) => {
			(box.GlobalPosition, box.GlobalRotation) = (pos, rot);
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

		childTransform.PositionBindable.Value = box.GlobalPosition;
		childTransform.RotationBindable.Value = box.GlobalRotation;
	}
}
