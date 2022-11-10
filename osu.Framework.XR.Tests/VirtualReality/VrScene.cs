using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.XR.Testing.VirtualReality;
using osu.Framework.XR.VirtualReality;

namespace osu.Framework.XR.Tests.VirtualReality;

public class VrScene : BasicTestScene {
	[Cached(typeof(VrCompositor))]
	public readonly VrCompositor VrCompositor;
	[Cached]
	public readonly VrResourceStore VrResources = new();

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
		}

		Add( VrCompositor );
		Scene.Add( new VrPlayer() );	
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );
		VrResources.Dispose();
	}

	/// <summary>
	/// This determines whether the testing will be done in VR or a simulated setup.
	/// <see langword="true"/> for simulated setup
	/// </summary>
	public virtual bool USE_VR_RIG => true;
}
