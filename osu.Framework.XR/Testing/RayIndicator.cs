using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Graphics.Lines;
using osu.Framework.XR.Graphics.Rendering;

namespace osu.Framework.XR.Testing;

public class RayIndicator : CompositeDrawable {
	public readonly BindableBool AllowDraggingBindable = new BindableBool( true );
	public bool AllowDragging {
		get => AllowDraggingBindable.Value;
		set => AllowDraggingBindable.Value = value;
	}

	private BindableWithCurrent<Vector3> a = new BindableWithCurrent<Vector3>();
	private BindableWithCurrent<Vector3> b = new BindableWithCurrent<Vector3>();

	public Bindable<Vector3> OriginCurrent {
		get => a;
		set => a.Current = value;
	}
	public Bindable<Vector3> LookCurrent {
		get => b;
		set => b.Current = value;
	}

	public Vector3 Direction => a.Value == b.Value ? Vector3.UnitY : ( b.Value - a.Value ).Normalized();

	Path3D line;

	public readonly BindableBool IsBidirectionalBindable = new( false );
	public bool IsBidirectional {
		get => IsBidirectionalBindable.Value;
		set => IsBidirectionalBindable.Value = value;
	}

	public RayIndicator ( Scene scene ) {
		AddRangeInternal( new Drawable[] {
				new PointIndicator( scene ) {
					Current = OriginCurrent,
				},
				new PointIndicator( scene ) {
					Current = LookCurrent,
					Alpha = 0.4f
				}
			} );

		foreach ( var i in InternalChildren.OfType<PointIndicator>() )
			i.AllowDraggingBindable.BindTo( AllowDraggingBindable );

		scene.Add( line = new Path3D() );

		(OriginCurrent, LookCurrent, IsBidirectionalBindable).BindValuesChanged( ( a, b, bi ) => {
			line.ClearNodes();
			if ( bi ) {
				line.AddNode( a - Direction * 100 );
				line.AddNode( b + Direction * 100 );
			}
			else {
				line.AddNode( a );
				line.AddNode( a + Direction * 100 );
			}
		}, true );

		AlwaysPresent = true;
	}

	public Colour4 Tint {
		get => line.Colour;
		set => line.Colour = value;
	}

	public Kind Kind {
		set {
			AllowDragging = value.IsEditable();
			Colour = value.MainColour();
			Tint = value.AccentColour();
		}
	}
}