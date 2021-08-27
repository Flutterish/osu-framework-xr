using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Projection;
using osuTK;

namespace osu.Framework.XR.Tests.Components {
	public class PointIndicator : CompositeDrawable {
		LayoutValue<Vector2> parentSize = new LayoutValue<Vector2>( Invalidation.DrawSize, InvalidationSource.Parent );

		readonly BindableWithCurrent<Vector3> current = new( Vector3.Zero );
		public Bindable<Vector3> Current {
			get => current;
			set => current.Current = value;
		}

		public readonly BindableBool AllowDraggingBindable = new BindableBool( true );
		public bool AllowDragging {
			get => AllowDraggingBindable.Value;
			set => AllowDraggingBindable.Value = value;
		}

		private Camera camera => scene.Camera;
		private Scene scene;
		private Drawable indicator;
		public PointIndicator ( Scene scene ) {
			AddInternal( indicator = new Circle {
				Size = new Vector2( 20, 20 ),
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre
			} );

			AutoSizeAxes = Axes.Both;
			Origin = Anchor.Centre;

			this.scene = scene;
			AddLayout( parentSize );
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			Current.BindValueChanged( v => {
				updatePosition();
			}, true );
		}

		protected override bool OnInvalidate ( Invalidation invalidation, InvalidationSource source ) {
			updatePosition();

			return base.OnInvalidate( invalidation, source );
		}

		void updatePosition () {
			if ( camera.Project( Current.Value, scene.DrawWidth, scene.DrawHeight, out var pos ) ) {
				Show();
				Position = pos;
			}
			else {
				Hide();
			}
		}

		bool isDragged = false;
		protected override bool OnHover ( HoverEvent e ) {
			if ( !isDragged && AllowDragging ) {
				indicator.ScaleTo( 1.4f, 150, Easing.Out );
				return true;
			}
			return false;
		}

		protected override bool OnDragStart ( DragStartEvent e ) {
			if ( AllowDragging ) {
				indicator.ScaleTo( 1.4f, 150, Easing.Out );
				isDragged = true;
				return true;
			}
			return false;
		}

		protected override void OnDrag ( DragEvent e ) {
			if ( AllowDragging ) {
				var pos = camera.Position + camera.DirectionOf( e.MousePosition, scene.DrawWidth, scene.DrawHeight );
				Current.Value = pos;
			}
			else {
				isDragged = false;
			}
		}

		protected override void OnDragEnd ( DragEndEvent e ) {
			indicator.ScaleTo( 1, 150, Easing.In );
			isDragged = false;
		}

		protected override void OnHoverLost ( HoverLostEvent e ) {
			if ( !isDragged ) indicator.ScaleTo( 1, 150, Easing.In );
		}
	}
}
