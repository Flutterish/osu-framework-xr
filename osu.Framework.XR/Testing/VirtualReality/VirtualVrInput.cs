using OpenVR.NET.Manifest;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;

namespace osu.Framework.XR.Testing.VirtualReality;

public class VirtualVrInput : VrInput {
	public VirtualVrInput ( VrCompositor vr ) : base( vr ) {
		factory = base.Factory.ToDictionary( k => k.Key, v => (Func<Enum, VrInput, Controller?, VrAction>)((a, b, c) => register( v.Value(a, b, c) )) );
		factory[typeof(PoseAction)] = (a, b, c) => register( new VirtualPoseAction( a, this, !CreatedActions.Any( x => x is PoseAction ), c ) );
	}

	public readonly Bindable<Vector3> LeftHandPosition = new();
	public readonly Bindable<Quaternion> LeftHandRotation = new( Quaternion.Identity );
	public readonly Bindable<Vector3> RightHandPosition = new();
	public readonly Bindable<Quaternion> RightHandRotation = new( Quaternion.Identity );

	protected readonly BindableList<VrAction> CreatedActions = new();
	VrAction register ( VrAction value ) {
		CreatedActions.Add( value );
		return value;
	}

	Dictionary<Type, Func<Enum, VrInput, Controller?, VrAction>> factory;
	protected override Dictionary<Type, Func<Enum, VrInput, Controller?, VrAction>> Factory => factory;

	public virtual Container<Drawable> CreateControlsDrawable ()
		=> new ControlsDrawable( CreatedActions );

	protected override void ApplyActionManifest ( IActionManifest manifest, Action callback ) { }

	class ControlsDrawable : FillFlowContainer {
		BindableList<VrAction> actions = new();
		public ControlsDrawable ( BindableList<VrAction> actions ) {
			this.actions.BindTo( actions );
			Direction = FillDirection.Vertical;
			Spacing = new( 10 );

			actions.BindCollectionChanged( (_, e) => {
				if ( e.NewItems == null )
					return;

				FillFlowContainer vectorControl ( int axes ) {
					var c = new FillFlowContainer();
					c.RelativeSizeAxes = Axes.X;
					c.AutoSizeAxes = Axes.Y;
					c.Spacing = new( 2 );

					c.Add( new BasicButton { Text = "Reset", RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y }.With( b => {
						b.Action = () => {
							for ( int i = 1; i <= axes; i++ ) {
								((BasicSliderBar<float>)c[i]).Current.SetDefault();
							}
						};
					} ) );

					for ( int i = 0; i < axes; i++ ) {
						var slider = new BasicSliderBar<float> { Current = new BindableFloat { MinValue = -1, MaxValue = 1 } }.With( c => {
							c.RelativeSizeAxes = Axes.X;
							c.Height = 10;
						} );
						c.Add( slider );
					}

					return c;
				}

				foreach ( VrAction i in e.NewItems ) {
					Add( labelled( i switch {
						BooleanAction button => new BasicCheckbox().With( c => {
							button.Value.BindTo( c.Current );
						} ),
						ScalarAction scalar => new BasicSliderBar<float> { Current = new BindableFloat { MinValue = 0, MaxValue = 1 } }.With( c => {
							c.RelativeSizeAxes = Axes.X;
							c.Height = 10;
							scalar.Value.BindTo( c.Current );
						} ),
						Vector2Action vec2 => vectorControl( 2 ).With( c => {
							( (BasicSliderBar<float>)c[1] ).Current.BindValueChanged( v => vec2.Value.Value = vec2.Value.Value with { X = v.NewValue } );
							( (BasicSliderBar<float>)c[2] ).Current.BindValueChanged( v => vec2.Value.Value = vec2.Value.Value with { Y = v.NewValue } );
						} ),
						Vector3Action vec3 => vectorControl( 3 ).With( c => {
							( (BasicSliderBar<float>)c[1] ).Current.BindValueChanged( v => vec3.Value.Value = vec3.Value.Value with { X = v.NewValue } );
							( (BasicSliderBar<float>)c[2] ).Current.BindValueChanged( v => vec3.Value.Value = vec3.Value.Value with { Y = v.NewValue } );
							( (BasicSliderBar<float>)c[3] ).Current.BindValueChanged( v => vec3.Value.Value = vec3.Value.Value with { Z = v.NewValue } );
						} ),
						PoseAction pose => new SpriteText { Text = $"In-scene attached to skeleton rig" },
						_ => new SpriteText { Text = $"{i.GetType().ReadableName()} Control Not Implemented" }
					}, $"{i.Name} - [{i.Source?.Role.ToString() ?? "Global"}]" ) );
				}
			}, true );
		}

		Drawable labelled ( Drawable drawable, LocalisableString label ) {
			if ( drawable is BasicCheckbox c ) {
				c.LabelText = label;
				return c;
			}

			return new FillFlowContainer {
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Direction = FillDirection.Vertical,
				Spacing = new( 5 ),
				Children = new Drawable[] {
					new SpriteText { Text = label },
					drawable
				}
			};
		}
	}

	class VirtualPoseAction : PoseAction {
		VirtualVrInput input;
		bool isLeft;
		public VirtualPoseAction ( object name, VirtualVrInput input, bool isLeft, Controller? source = null ) : base( name, source ) {
			this.input = input;
			this.isLeft = isLeft;
		}

		public override OpenVR.NET.Input.PoseInput? FetchData () {
			var pos = (isLeft ? input.LeftHandPosition : input.RightHandPosition).Value;
			var rot = (isLeft ? input.LeftHandRotation : input.RightHandRotation).Value;

			return new OpenVR.NET.Input.PoseInput {
				Position = new( pos.X, pos.Y, pos.Z ),
				Rotation = new( rot.X, rot.Y, rot.Z, rot.W )
			};
		}

		public override OpenVR.NET.Input.PoseInput? FetchDataForNextFrame () => FetchData();
		public override OpenVR.NET.Input.PoseInput? FetchDataForPrediction ( float secondsFromNow ) => FetchData();
	}
}
