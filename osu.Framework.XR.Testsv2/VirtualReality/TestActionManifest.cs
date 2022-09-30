using OpenVR.NET.Manifest;

namespace osu.Framework.XR.Tests.VirtualReality;

public enum TestingCategory {
	All
}

public enum TestingAction {
	HandLeft,
	HandRight
}

public static class TestActionManifest {
	public static readonly ActionManifest<TestingCategory, TestingAction> Value = new() {
		ActionSets = new() {
			new() { Name = TestingCategory.All, Type = ActionSetType.Single }
		},
		Actions = new() {
			new() { Category = TestingCategory.All, Name = TestingAction.HandLeft, Type = ActionType.LeftHandSkeleton },
			new() { Category = TestingCategory.All, Name = TestingAction.HandRight, Type = ActionType.RightHandSkeleton }
		}
	};
}