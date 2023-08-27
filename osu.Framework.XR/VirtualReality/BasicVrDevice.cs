using OpenVR.NET.Devices;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Maths;
using Controller = osu.Framework.XR.VirtualReality.Devices.Controller;
using VrDevice = osu.Framework.XR.VirtualReality.Devices.VrDevice;

namespace osu.Framework.XR.VirtualReality;

public partial class BasicVrDevice : CompositeDrawable3D {
	List<BasicVrDeviceComponent> components = new();
	List<BasicVrDeviceComponent> references = new();
	public IReadOnlyList<BasicVrDeviceComponent> Components => components;
	public IReadOnlyList<BasicVrDeviceComponent> References => references;
	public readonly VrDevice Source;
	public BasicVrDevice ( VrDevice source ) {
		Source = source;
	}

	[Resolved]
	VrCompositor compositor { get; set; } = null!;

	public readonly BindableBool UseRealtimePosition = new( true );

	[BackgroundDependencyLoader]
	private void load ( IRenderer renderer, VrResourceStore resources ) {
		if ( Source.Model is DeviceModel model ) {
			foreach ( var i in model.Components ) {
				resources.LoadComponent( i, renderer, t => t is ComponentModel.ComponentType.Component or ComponentModel.ComponentType.ReferencePoint ).ContinueWith( r => {
					if ( r.Result is not var (tx, mesh, type) )
						return;

					BasicVrDeviceComponent child = new( Source, i ) { Mesh = mesh! };
					child.UseRealtimePosition.BindTo( UseRealtimePosition );

					if ( tx != null ) {
						child.OnLoadComplete += c => {
							child.Material.SetTexture( UnlitMaterial.Texture, tx );
						};
					}
					Schedule( () => {
						AddInternal( child );
						if ( type is ComponentModel.ComponentType.Component )
							components.Add( child );
						else
							references.Add( child );
					} );
				} );
			}
		}
	}

	protected override void Update () {
		Position = compositor.ActivePlayer?.InGlobalSpace( Source.Position ) ?? Source.Position;
		Rotation = compositor.ActivePlayer?.InGlobalSpace( Source.Rotation ) ?? Source.Rotation;

		foreach ( var i in components ) {
			i.RenderLayer = RenderLayer;
		}
	}

	public partial class BasicVrDeviceComponent : BasicModel {
		public readonly ComponentModel Source;
		public readonly VrDevice Device;
		public BasicVrDeviceComponent ( VrDevice device, ComponentModel source ) {
			Source = source;
			Device = device;

			UseRealtimePosition.BindValueChanged( v => Invalidate( Invalidation.DrawNode ) );
		}

		[Resolved]
		VrCompositor compositor { get; set; } = null!;

		public readonly BindableBool UseRealtimePosition = new( false );

		bool wasVisible = false;
		bool isVisible;
		void updateVisibility () {
			if ( wasVisible != isVisible )
				Invalidate( Invalidation.DrawNode );

			wasVisible = isVisible;
		}
		protected override void Update () {
			isVisible = Device.IsEnabled.Value;
			var maybeState = ( Device as Controller )?.GetComponentState( Source );
			if ( maybeState is not OpenVR.NET.Devices.Controller.ComponentState state ) {
				updateVisibility();
				return;
			}

			Position = state.Position.ToOsuTk();
			Rotation = state.Rotation.ToOsuTk();
			isVisible &= state.Properties.HasFlag( Valve.VR.EVRComponentProperty.IsVisible );
			updateVisibility();
		}

		protected override ModelDrawNode? CreateDrawNode3D ( int index )
			=> new DrawNode( this, index );

		class DrawNode : ModelDrawNode {
			new BasicVrDeviceComponent Source => (BasicVrDeviceComponent)base.Source;

			Matrix4 parentMatrix;
			bool isVisible;
			bool isRealtime;
			public DrawNode ( BasicVrDeviceComponent source, int index ) : base( source, index ) {
				parentMatrix = source.Parent!.Parent?.Matrix ?? Matrix4.Identity;
			}

			Vector3 positionOffset;
			Quaternion rotationOffset;
			protected override void UpdateState () {
				base.UpdateState();
				isVisible = Source.isVisible;
				isRealtime = Source.UseRealtimePosition.Value;
				positionOffset = Source.compositor.ActivePlayer?.PositionOffset ?? Vector3.Zero;
				rotationOffset = Source.compositor.ActivePlayer?.RotationOffset ?? Quaternion.Identity;
			}

			public override void Draw ( IRenderer renderer, object? ctx = null ) {
				if ( !isVisible )
					return;

				if ( isRealtime ) {
					Matrix = parentMatrix;

					var maybeState = ( Source.Device as Controller )?.GetComponentState( Source.Source );
					if ( maybeState is OpenVR.NET.Devices.Controller.ComponentState state ) {
						Matrix *= Matrix4.CreateFromQuaternion( state.Rotation.ToOsuTk() )
								* Matrix4.CreateTranslation( state.Position.ToOsuTk() );
					}

					Matrix *= Matrix4.CreateFromQuaternion( rotationOffset * Source.Device.RenderRotation )
							* Matrix4.CreateTranslation( rotationOffset.Apply( Source.Device.RenderPosition ) + positionOffset );
				}
				
				base.Draw( renderer, ctx );
			}
		}
	}
}
