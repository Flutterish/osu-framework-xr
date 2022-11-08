using OpenVR.NET.Input;
using OpenVR.NET.Manifest;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;
using osu.Framework.XR.VirtualReality;
using PoseAction = osu.Framework.XR.VirtualReality.PoseAction;

namespace osu.Framework.XR.Tests.VirtualReality;

public class TestScenePose : VrScene {
	BasicModel a;
	BasicModel b;

	public TestScenePose () {
		Scene.Add( a = new() { Mesh = BasicMesh.UnitCube, Scale = new( 0.05f ) } );
		Scene.Add( b = new() { Mesh = BasicMesh.UnitCube, Scale = new( 0.05f ) } );

		VrCompositor.Input.SetActionManifest( new ActionManifest<TestingCategory, TestingAction> {
			ActionSets = new() {
				new() { Name = TestingCategory.All, Type = ActionSetType.LeftRight }
			},
			Actions = new() {
				new() { Category = TestingCategory.All, Name = TestingAction.PoseA, Type = ActionType.Pose },
				new() { Category = TestingCategory.All, Name = TestingAction.PoseB, Type = ActionType.Pose }
			}
		} );

		var poseA = VrCompositor.Input.GetAction<PoseAction>( TestingAction.PoseA )!;
		var poseB = VrCompositor.Input.GetAction<PoseAction>( TestingAction.PoseB )!;

		a.OnUpdate += _ => {
			if ( poseA.FetchData() is PoseInput pose ) {
				a.Position = pose.Position.ToOsuTk();
				a.Rotation = pose.Rotation.ToOsuTk();
			}
		};
		b.OnUpdate += _ => {
			if ( poseB.FetchData() is PoseInput pose ) {
				b.Position = pose.Position.ToOsuTk();
				b.Rotation = pose.Rotation.ToOsuTk();
			}
		};
	}
}
