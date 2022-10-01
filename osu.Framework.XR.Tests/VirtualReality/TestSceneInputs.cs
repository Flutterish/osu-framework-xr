using OpenVR.NET.Devices;
using OpenVR.NET.Input;
using OpenVR.NET.Manifest;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.VirtualReality;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.VirtualReality;
public class TestSceneInputs : VrScene {
	Panel panel;
	FillFlowContainer flow;

	public TestSceneInputs () {
		Scene.Add( panel = new() { ContentSize = new( 500 ), Z = 5 } );
		panel.Content.Add( new Box { RelativeSizeAxes = Framework.Graphics.Axes.Both, Colour = Color4.Black } );
		panel.Content.Add( flow = new() {
			Direction = FillDirection.Vertical,
			AutoSizeAxes = Framework.Graphics.Axes.Both
		} );

		VrCompositor.Initialized += vr => {
			vr.SetActionManifest( new ActionManifest<TestingCategory, TestingAction> {
				ActionSets = new() {
					new() { Name = TestingCategory.All, Type = ActionSetType.LeftRight }
				},
				Actions = new() {
					new() { Category = TestingCategory.All, Name = TestingAction.Boolean, Type = ActionType.Boolean },
					new() { Category = TestingCategory.All, Name = TestingAction.Scalar, Type = ActionType.Scalar },
					new() { Category = TestingCategory.All, Name = TestingAction.Vector2, Type = ActionType.Vector2 },
					new() { Category = TestingCategory.All, Name = TestingAction.Vector3, Type = ActionType.Vector3 }
				}
			} );

			vr.DeviceDetected += onVrDeviceDetected;
			foreach ( var i in vr.TrackedDevices )
				onVrDeviceDetected( i );

			vr.BindActionsLoaded( () => {
				void add<T, Tcomp> ( TestingAction action ) where T : struct where Tcomp : InputAction<T> {
					var comp = vr.GetAction<Tcomp>( action )!;
					var text = new SpriteText();
					flow.Add( text );
					OnUpdate += _ => text.Text = $"{action} [Global] = {comp.Value}";
				}

				add<bool, BooleanAction>( TestingAction.Boolean );
				add<float, ScalarAction>( TestingAction.Scalar );
				add<System.Numerics.Vector2, Vector2Action>( TestingAction.Vector2 );
				add<System.Numerics.Vector3, Vector3Action>( TestingAction.Vector3 );
			} );
		};
	}

	void onVrDeviceDetected ( VrDevice device ) {
		if ( device is not Controller c )
			return;

		c.VR.BindActionsLoaded( () => {
			void add<T, Tcomp> ( TestingAction action ) where T : struct where Tcomp : InputAction<T> {
				var comp = c.GetAction<Tcomp>( action )!;
				var text = new SpriteText();
				flow.Add( text );
				OnUpdate += _ => text.Text = $"{action} [{c.Role}] = {comp.Value}";
			}

			add<bool, BooleanAction>( TestingAction.Boolean );
			add<float, ScalarAction>( TestingAction.Scalar );
			add<System.Numerics.Vector2, Vector2Action>( TestingAction.Vector2 );
			add<System.Numerics.Vector3, Vector3Action>( TestingAction.Vector3 );
		} );
	}
}
