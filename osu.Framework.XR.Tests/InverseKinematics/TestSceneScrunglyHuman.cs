using osu.Framework.XR.InverseKinematics;
using osuTK;

namespace osu.Framework.XR.Tests.InverseKinematics;

public partial class TestSceneScrunglyHuman : IkTestScene {
#nullable disable
	Joint HeadTip;
	Joint Neck;
	Joint LeftShoulder;
	Joint RightShoulder;
	Joint LeftElbow;
	Joint RightElbow;
	Joint LeftWrist;
	Joint RightWrist;
	Joint LeftHandTip;
	Joint RightHandTip;
	Joint SpineA;
	Joint SpineB;
	Joint Hips;
	Joint LeftHip;
	Joint RightHip;
	Joint LeftKnee;
	Joint RightKnee;
	Joint LeftHeel;
	Joint RightHeel;
#nullable restore
	protected override (Joint root, TargetCollection targets) CreateIkModel () {
		var targets = new TargetCollection();
		const float deg5 = 5f / 360 * float.Tau;
		const float deg10 = 10f / 360 * float.Tau;
		const float deg20 = 20f / 360 * float.Tau;
		const float deg30 = 30f / 360 * float.Tau;
		const float deg60 = 60f / 360 * float.Tau;
		const float deg70 = 70f / 360 * float.Tau;
		const float deg80 = 80f / 360 * float.Tau;
		const float deg90 = 90f / 360 * float.Tau;

		return (targets.NextTarget = HeadTip = new Joint { Child = new() {
			Length = 0.18f,
			Target = Neck = new HingeJoint {
				NeutralRotation = Quaternion.FromAxisAngle( Vector3.UnitX, deg90 ),
				AngleLimitLeftRight = new( 0 ),
				AngleLimitBackFront = new( deg10 ),
				Children = new Link[] {
					new() {
						Length = 0.2f,
						Target = LeftShoulder = new HingeJoint {
							NeutralRotation = Quaternion.FromAxisAngle( Vector3.UnitY, deg90 ),
							AngleLimitLeftRight = new( 0 ),
							AngleLimitBackFront = new( 0 ),
							Child = new() {
								Length = 0.32f,
								Target = LeftElbow = new HingeJoint {
									NeutralRotation = Quaternion.FromAxisAngle( Vector3.UnitY, -deg90 ),
									AngleLimitLeftRight = new( deg70 ),
									AngleLimitBackFront = new( deg70 ),
									Child = new() {
										Length = 0.32f,
										Target = targets.NextTarget = LeftWrist = new HingeJoint {
											AngleLimitLeftRight = new( deg70 ),
											AngleLimitBackFront = new( deg70 )
										}
									} 
								}
							} 
						}
					},
					new() {
						Length = 0.2f,
						Target = RightShoulder = new HingeJoint {
							NeutralRotation = Quaternion.FromAxisAngle( Vector3.UnitY, -deg90 ),
							AngleLimitLeftRight = new( 0 ),
							AngleLimitBackFront = new( 0 ),
							Child = new() {
								Length = 0.32f,
								Target = RightElbow = new HingeJoint {
									NeutralRotation = Quaternion.FromAxisAngle( Vector3.UnitY, deg90 ),
									AngleLimitLeftRight = new( deg70 ),
									AngleLimitBackFront = new( deg70 ),
									Child = new() {
										Length = 0.32f,
										Target = targets.NextTarget = RightWrist = new HingeJoint {
											AngleLimitLeftRight = new( deg70 ),
											AngleLimitBackFront = new( deg70 )
										}
									} 
								}
							} 
						}
					},
					new() {
						Length = 0.2f,
						Target = SpineA = new HingeJoint {
							AngleLimitLeftRight = new( 0 ),
							AngleLimitBackFront = new( 0 ),
							Child = new() {
								Length = 0.2f,
								Target = SpineB = new HingeJoint {
									AngleLimitLeftRight = new( 0 ),
									AngleLimitBackFront = new( 0 ),
									Child = new() {
										Length = 0.2f,
										Target = Hips = new HingeJoint {
											AngleLimitLeftRight = new( 0 ),
											AngleLimitBackFront = new( 0 ),
											Children = new Link[] {
												new() {
													Length = 0.15f,
													Target = LeftHip = new HingeJoint {
														NeutralRotation = Quaternion.FromAxisAngle( Vector3.UnitY, -deg90 ),
														AngleLimitLeftRight = new( 0 ),
														AngleLimitBackFront = new( 0 ),
														Child = new() {
															Length = 0.4f,
															Target = LeftKnee = new HingeJoint {
																NeutralRotation = Quaternion.FromAxisAngle( Vector3.UnitY, deg90 ),
																AngleLimitLeftRight = new( deg80 ),
																AngleLimitBackFront = new( deg80 ),
																Child = new() { 
																	Length = 0.4f,
																	Target = targets.NextTarget = LeftHeel = new HingeJoint {
																		AngleLimitLeftRight = new( deg70 ),
																		AngleLimitBackFront = new( deg70 ),
																	}
																} 
															}
														} 
													}
												},
												new() {
													Length = 0.15f,
													Target = RightHip = new HingeJoint {
														NeutralRotation = Quaternion.FromAxisAngle( Vector3.UnitY, deg90 ),
														AngleLimitLeftRight = new( 0 ),
														AngleLimitBackFront = new( 0 ),
														Child = new() {
															Length = 0.4f,
															Target = RightKnee = new HingeJoint {
																NeutralRotation = Quaternion.FromAxisAngle( Vector3.UnitY, -deg90 ),
																AngleLimitLeftRight = new( deg80 ),
																AngleLimitBackFront = new( deg80 ),
																Child = new() {
																	Length = 0.4f,
																	Target = targets.NextTarget = RightHeel = new HingeJoint {
																		AngleLimitLeftRight = new( deg70 ),
																		AngleLimitBackFront = new( deg70 ),
																	}
																} 
															}
														} 
													}
												}
											} 
										}
									} 
								}
							} 
						}
					}
				} 
			}
		} }, targets);
	}
}
