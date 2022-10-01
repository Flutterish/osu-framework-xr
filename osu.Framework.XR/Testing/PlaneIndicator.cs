using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Lines;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Testing;

public class PlaneIndicator : CompositeDrawable {
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

	public Vector3 Normal => a.Value == b.Value ? Vector3.UnitY : ( b.Value - a.Value ).Normalized();

	BasicModel plane;
	DashedPath3D line;

	public PlaneIndicator ( Scene scene ) {
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

		scene.Add( plane = new BasicModel() );
		scene.Add( line = new DashedPath3D() );

		(OriginCurrent, LookCurrent).BindValuesChanged( ( a, b ) => {
			var mesh = plane.Mesh;
			mesh.Clear();

			var normal = Normal;
			var orth = normal.AnyOrthogonal();
			var orth2 = Vector3.Cross( normal, orth );
			mesh.Vertices.Add( new() { Position = OriginCurrent.Value + orth + orth2 } );
			mesh.Vertices.Add( new() { Position = OriginCurrent.Value - orth + orth2 } );
			mesh.Vertices.Add( new() { Position = OriginCurrent.Value + orth - orth2 } );
			mesh.Vertices.Add( new() { Position = OriginCurrent.Value - orth - orth2 } );

			mesh.AddFace( 0, 1, 2 );
			mesh.AddFace( 3, 1, 2 );

			line.ClearNodes();
			for ( float t = 0; t <= 1; t += 0.2f ) {
				line.AddNode( a + ( b - a ) * t );
			}

			updateMesh = true;
		}, true );

		AlwaysPresent = true;
	}

	bool updateMesh;

	protected override void Update () {
		if ( updateMesh ) {
			plane.Mesh.CreateFullUpload().Enqueue();
			updateMesh = false;
		}

		base.Update();
		line.Colour = Colour;
	}

	public Colour4 Tint {
		get => plane.Colour;
		set => plane.Colour = value;
	}

	public Kind Kind {
		set {
			AllowDragging = value.IsEditable();
			Colour = value.MainColour();
			Tint = value.SecondaryColour();
		}
	}
}