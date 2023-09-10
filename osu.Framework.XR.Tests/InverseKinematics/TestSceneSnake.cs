using osu.Framework.XR.InverseKinematics;

namespace osu.Framework.XR.Tests.InverseKinematics;

public partial class TestSceneSnake : IkTestScene {
	protected override (Joint root, TargetCollection targets) CreateIkModel () {
		var targets = new TargetCollection();

		return ( targets.NextTarget = new Joint {
			Child = new() {
				Length = 1,
				Target = new Joint {
					Child = new() {
						Length = 1,
						Target = new Joint {
							Child = new() {
								Length = 1,
								Target = new Joint {
									Child = new() {
										Length = 1,
										Target = targets.NextTarget = new Joint { }
									}
								}
							}
						}
					}
				}
			}
		}, targets );
	}
}
