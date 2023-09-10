using osu.Framework.XR.InverseKinematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Tests.InverseKinematics;

public partial class TestSceneTree : IkTestScene {
	protected override (Joint root, TargetCollection targets) CreateIkModel () {
		var targets = new TargetCollection();

		return (targets.NextTarget = new Joint {
			Child = new() {
				Length = 1,
				Target = new Joint {
					Child = new() {
						Length = 1,
						Target = new Joint {
							Children = new Link[] {
								new() {
									Length = 1,
									Target = new Joint {
										Child = new() {
											Length = 1,
											Target = targets.NextTarget = new Joint()
										}
									}
								},
								new() {
									Length = 1,
									Target = new Joint {
										Child = new() {
											Length = 1,
											Target = targets.NextTarget = new Joint()
										}
									}
								},
								new() {
									Length = 1,
									Target = new Joint {
										Child = new() {
											Length = 1,
											Target = targets.NextTarget = new Joint()
										}
									}
								}
							}
						}
					}
				}
			}
		}, targets);
	}
}
