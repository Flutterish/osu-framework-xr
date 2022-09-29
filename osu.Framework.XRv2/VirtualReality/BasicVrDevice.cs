using OpenVR.NET.Devices;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.VirtualReality;

public class BasicVrDevice : CompositeDrawable3D {
	List<BasicVrDeviceComponent> components = new();
	List<BasicVrDeviceComponent> references = new();
	public IEnumerable<BasicVrDeviceComponent> Components => components;
	public IEnumerable<BasicVrDeviceComponent> References => references;
	public readonly VrDevice Source;
	public BasicVrDevice ( VrDevice source ) {
		Source = source;
	}

	[BackgroundDependencyLoader]
	private void load ( IRenderer renderer, VrResourceStore resources ) {
		if ( Source.Model is DeviceModel model ) {
			foreach ( var i in model.Components ) {
				BasicMesh mesh = null!;
				BasicVrDeviceComponent child = null!;
				i.LoadAsync(
					begin: type => {
						if ( type is ComponentModel.ComponentType.Component ) {
							child = new BasicVrDeviceComponent( Source, i ) { Mesh = mesh = new() };
							components.Add( child );
							return true;
						}
						else if ( type is ComponentModel.ComponentType.ReferencePoint ) {
							child = new BasicVrDeviceComponent( Source, i ) { Mesh = mesh = new() };
							references.Add( child );
							return true;
						}
						return false;
					},
					finish: type => {
						mesh.CreateFullUnsafeUpload().Enqueue();
						Schedule( () => AddInternal( child ) );
					},
					addVertice: ( pos, norm, uv ) => mesh.VertexBuffer.Data.Add( new() {
						Position = pos.ToOsuTk(),
						Normal = norm.ToOsuTk(),
						UV = uv.ToOsuTk()
					} ),
					addTriangle: ( a, b, c ) => mesh.AddFace( (uint)a, (uint)b, (uint)c ),
					addTexture: async tex => {
						var tx = await resources.Textures.GetOrAdd( tex.ID, async _ => {
							var image = await tex.LoadImage( flipVertically: true );
							if ( image == null )
								return null;

							var tx = renderer.CreateTexture( image.Width, image.Height );
							tx.SetData( new TextureUpload( image ) );
							return tx;
						} );

						if ( tx is null )
							return;

						Schedule( () => {
							if ( child.IsLoaded ) {
								child.Material.SetTexture( "tex", tx );
								child.Material.Set( "useGamma", true );
							}
							else {
								child.OnLoadComplete += c => {
									child.Material.SetTexture( "tex", tx );
									child.Material.Set( "useGamma", true );
								};
							}
						} );
					}
				);
			}
		}
	}

	protected override void Update () {
		Position = Source.Position.ToOsuTk();
		Rotation = Source.Rotation.ToOsuTk();
	}

	public class BasicVrDeviceComponent : BasicModel {
		public readonly ComponentModel Source;
		public readonly VrDevice Device;
		public BasicVrDeviceComponent ( VrDevice device, ComponentModel source ) {
			Source = source;
			Device = device;
		}

		bool wasVisible = false;
		bool isVisible;
		void updateVisibility () {
			if ( wasVisible != isVisible )
				Invalidate( Invalidation.DrawNode );

			wasVisible = isVisible;
		}
		protected override void Update () {
			isVisible = Device.IsEnabled;
			var maybeState = ( Device as Controller )?.GetComponentState( Source );
			if ( maybeState is not Controller.ComponentState state ) {
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
			public DrawNode ( BasicVrDeviceComponent source, int index ) : base( source, index ) {
				parentMatrix = source.Parent!.Parent?.Matrix ?? Matrix4.Identity;
			}

			protected override void UpdateState () {
				base.UpdateState();
				isVisible = Source.isVisible;
			}

			public override void Draw ( IRenderer renderer, object? ctx = null ) {
				if ( !isVisible )
					return;

				Matrix = parentMatrix;

				var maybeState = ( Source.Device as Controller )?.GetComponentState( Source.Source );
				if ( maybeState is Controller.ComponentState state ) {
					Matrix *= Matrix4.CreateFromQuaternion( state.Rotation.ToOsuTk() ) 
							* Matrix4.CreateTranslation( state.Position.ToOsuTk() );
				}

				Matrix *= Matrix4.CreateFromQuaternion( Source.Device.RenderRotation.ToOsuTk() )
						* Matrix4.CreateTranslation( Source.Device.RenderPosition.ToOsuTk() );

				base.Draw( renderer, ctx );
			}
		}
	}
}
