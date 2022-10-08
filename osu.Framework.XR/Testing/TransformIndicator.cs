using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Testing;

public class TransformIndicator : CompositeDrawable {
	public readonly BindableBool AllowDraggingBindable = new BindableBool( true );
	public bool AllowDragging {
		get => AllowDraggingBindable.Value;
		set => AllowDraggingBindable.Value = value;
	}

	private readonly BindableWithCurrent<Vector3> pos = new BindableWithCurrent<Vector3>();
	private readonly BindableWithCurrent<Quaternion> rot = new BindableWithCurrent<Quaternion>() { Value = Quaternion.Identity };

	public Bindable<Vector3> PositionBindable {
		get => pos;
		set => pos.Current = value;
	}
	public Bindable<Quaternion> RotationBindable {
		get => rot;
		set => rot.Current = value;
	}

	PointIndicator lookDirection;
	DashedLineVisual lookLine;
	public TransformIndicator ( Scene scene ) {
		AddInternal( new PointIndicator( scene ) {
			Current = PositionBindable
		} );
		AddInternal( lookDirection = new PointIndicator( scene ) );
		lookDirection.Current.Value = Vector3.UnitZ * 2;

		bool rotLock = false;
		rot.BindValueChanged( v => {
			if ( rotLock )
				return;

			rotLock = true;
			lookDirection.Current.Value = pos.Value + v.NewValue.Apply( Vector3.UnitZ * (lookDirection.Current.Value - pos.Value).Length );
			rotLock = false;
		}, true );
		pos.BindValueChanged( v => {
			if ( rotLock )
				return;

			rotLock = true;
			lookDirection.Current.Value += v.NewValue - v.OldValue;
			rotLock = false;
		} );
		lookDirection.Current.BindValueChanged( v => {
			if ( rotLock )
				return;

			rotLock = true;
			rot.Current.Value = ( v.NewValue - pos.Value ).LookRotation();
			rotLock = false;
		} );

		AddInternal( lookLine = new( scene ) );
		(pos, lookDirection.Current).BindValuesChanged( (a, b) => {
			lookLine.PointA.Value = a;
			lookLine.PointB.Value = b;
		}, true );
	}

	public Colour4 Tint {
		get => lookLine.Colour;
		set {
			lookLine.Colour = value;
		}
	}

	public Kind Kind {
		set {
			AllowDragging = value.IsEditable();
			Colour = value.MainColour();
			Tint = value.AccentColour();
		}
	}
}
