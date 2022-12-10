using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Testing.VirtualReality;
using osu.Framework.XR.VirtualReality;
using osuTK;

namespace osu.Framework.XR.Tests.VirtualReality;

public partial class VrScene : BasicTestScene {
	[Cached(typeof(VrCompositor))]
	public readonly VrCompositor VrCompositor;
	[Cached]
	public readonly VrResourceStore VrResources = new();

	protected readonly TrackedVrPlayer player;
	public VrScene () {
		if ( USE_VR_RIG ) {
			var comp = new TestingVrCompositor();
			VrCompositor = comp;
			TestingRig rig = new( Scene );
			Add( rig );

			comp.AddRig( rig );

			var controls = comp.Input.CreateControlsDrawable();
			controls.AutoSizeAxes = Axes.Y;
			controls.RelativeSizeAxes = Axes.X;
			Add( new Container {
				RelativeSizeAxes = Axes.Both,
				Size = new( 0.4f, 0.5f ),
				Origin = Anchor.BottomRight,
				Anchor = Anchor.BottomRight,
				Children = new Drawable[] {
					new Box { Colour = FrameworkColour.GreenDark, RelativeSizeAxes = Axes.Both },
					new BasicScrollContainer {
						RelativeSizeAxes = Axes.Both,
						Padding = new MarginPadding( 16 ),
						ScrollbarVisible = false,
						Child = controls
					}
				}
			} );
		}
		else {
			VrCompositor = new();
			AddLabel( "Vr Player" );
			AddSliderStep( "Player Offset X", -4f, 4, 0, v => player!.PositionOffset = player.PositionOffset with { X = v } );
			AddSliderStep( "Player Offset Y", -4f, 4, 0, v => player!.PositionOffset = player.PositionOffset with { Y = v } );
			AddSliderStep( "Player Offset Z", -4f, 4, 0, v => player!.PositionOffset = player.PositionOffset with { Z = v } );
			AddSliderStep( "Player Euler X (Exclusive)", -4f, 4, 0, v => player!.RotationOffset = Quaternion.FromEulerAngles( v, 0, 0 ) );
			AddSliderStep( "Player Euler Y (Exclusive)", -4f, 4, 0, v => player!.RotationOffset = Quaternion.FromEulerAngles( 0, v, 0 ) );
			AddSliderStep( "Player Euler Z (Exclusive)", -4f, 4, 0, v => player!.RotationOffset = Quaternion.FromEulerAngles( 0, 0, v ) );
			AddLabel( "Test" );
		}

		Add( VrCompositor );
		Scene.Add( player = new TrackedVrPlayer() );	
	}

	public partial class TrackedVrPlayer : VrPlayer {
		public TrackedVrPlayer () {
			AddInternal( new BasicModel { Mesh = BasicMesh.UnitCube, Scale = new(0.1f) } );
		}
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );
		VrResources.Dispose();
	}

	/// <summary>
	/// This determines whether the testing will be done in VR or a simulated setup.
	/// <see langword="true"/> for simulated setup
	/// </summary>
	public virtual bool USE_VR_RIG => false;
}
