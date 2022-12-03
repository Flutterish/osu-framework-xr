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
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.XR.Testing.VirtualReality;

public class VirtualVrInput : VrInput {
	public VirtualVrInput ( VrCompositor vr ) : base( vr ) {
		factory = base.Factory.ToDictionary( k => k.Key, v => (Func<Enum, VrInput, Controller?, VrAction>)((a, b, c) => register( v.Value(a, b, c) )) );
		factory[typeof(PoseAction)] = (a, b, c) => register( createPose( a, c ) );
	}

	VirtualPoseAction createPose ( Enum name, Controller? source ) {
		var (pos, rot) = GetPoseSource( name );

		return new VirtualPoseAction( name, pos, rot, source );
	}

	protected virtual (Bindable<Vector3>, Bindable<Quaternion>) GetPoseSource ( Enum name ) {
		return createdActions.Any( x => x is PoseAction ) ? ( RightHandPosition, RightHandRotation ) : ( LeftHandPosition, LeftHandRotation );
	}

	public readonly Bindable<Vector3> LeftHandPosition = new();
	public readonly Bindable<Quaternion> LeftHandRotation = new( Quaternion.Identity );
	public readonly Bindable<Vector3> RightHandPosition = new();
	public readonly Bindable<Quaternion> RightHandRotation = new( Quaternion.Identity );

	Dictionary<object, VrAction> aggregateActions = new();
	Dictionary<object, List<VrAction>> sourcedActions = new();

	IEnumerable<T> sourcesFor<T> ( T global ) where T : VrAction {
		if ( sourcedActions.TryGetValue( global.Name, out var list ) )
			return list.OfType<T>();
		return Array.Empty<T>();
	}

	bool aggregateSourceFor<T> ( T source, [NotNullWhen(true)] out T? aggregate ) where T : VrAction {
		if ( aggregateActions.TryGetValue( source.Name, out var global ) ) {
			aggregate = (T)global;
			return true;
		}
		else {
			aggregate = null;
			return false;
		}
	}

	BindableList<VrAction> createdActions = new();
	VrAction register ( VrAction value ) {
		if ( value.Source == null ) {
			aggregateActions.Add( value.Name, value );

			void linkSources<T, V> ( T global, Func<V, V, V> coercer ) where T : VrAction, IVrInputAction<V> {
				global.ValueBindable.BindValueChanged( v => {
					foreach ( var i in sourcesFor( global ) ) {
						i.Value = coercer( i.Value, v.NewValue );
					}
				}, true );
			}

			if ( value is BooleanAction button )
				linkSources( button, (bool local, bool global) => local && global );
			else if ( value is ScalarAction scalar )
				linkSources( scalar, (float local, float global) => Math.Min( local, global ) );
			else if ( value is Vector2Action vec2 )
				linkSources( vec2, (Vector2 local, Vector2 global) => new Vector2(
					x: MathF.CopySign( Math.Min( Math.Abs( local.X ), Math.Abs( global.X ) ), local.X ),
					y: MathF.CopySign( Math.Min( Math.Abs( local.Y ), Math.Abs( global.Y ) ), local.Y )
				) );
			else if ( value is Vector3Action vec3 )
				linkSources( vec3, ( Vector3 local, Vector3 global ) => new Vector3(
					x: MathF.CopySign( Math.Min( Math.Abs( local.X ), Math.Abs( global.X ) ), local.X ),
					y: MathF.CopySign( Math.Min( Math.Abs( local.Y ), Math.Abs( global.Y ) ), local.Y ),
					z: MathF.CopySign( Math.Min( Math.Abs( local.Z ), Math.Abs( global.Z ) ), local.Z )
				) );
		}
		else {
			if ( !sourcedActions.TryGetValue( value.Name, out var list ) )
				sourcedActions.Add( value.Name, list = new() );
			list.Add( value );

			void linkSources<T, V> ( T watcher, Func<IEnumerable<T>, V> aggregator ) where T : VrAction, IVrInputAction<V> {
				watcher.ValueBindable.BindValueChanged( v => {
					if ( aggregateSourceFor( watcher, out var global ) ) {
						global.Value = aggregator( sourcesFor( global ) );
					}
				}, true );
				if ( aggregateSourceFor( watcher, out var global ) ) {
					((Bindable<V>)global.ValueBindable).TriggerChange();
				}
			}

			if ( value is BooleanAction button )
				linkSources( button, s => s.Any( x => x.Value ) );
			else if ( value is ScalarAction scalar )
				linkSources( scalar, s => s.Max( x => x.Value ) );
			else if ( value is Vector2Action vec2 )
				linkSources( vec2, s => new Vector2(
					x: s.MaxBy( x => Math.Abs( x.Value.X ) )!.Value.X,
					y: s.MaxBy( x => Math.Abs( x.Value.Y ) )!.Value.Y
				) );
			else if ( value is Vector3Action vec3 )
				linkSources( vec3, s => new Vector3(
					x: s.MaxBy( x => Math.Abs( x.Value.X ) )!.Value.X,
					y: s.MaxBy( x => Math.Abs( x.Value.Y ) )!.Value.Y,
					z: s.MaxBy( x => Math.Abs( x.Value.Z ) )!.Value.Z
				) );
		}
		
		createdActions.Add( value );
		return value;
	}

	Dictionary<Type, Func<Enum, VrInput, Controller?, VrAction>> factory;
	protected override Dictionary<Type, Func<Enum, VrInput, Controller?, VrAction>> Factory => factory;

	public virtual Container<Drawable> CreateControlsDrawable ()
		=> new ControlsDrawable( createdActions );

	protected override void ApplyActionManifest ( IActionManifest manifest, Action callback ) { }

	partial class ControlsDrawable : FillFlowContainer {
		BindableList<VrAction> actions = new();
		List<IBindable> bindables = new();
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
							button.ValueBindable.BindTo( c.Current );
						} ),
						ScalarAction scalar => new BasicSliderBar<float> { Current = new BindableFloat { MinValue = 0, MaxValue = 1 } }.With( c => {
							c.RelativeSizeAxes = Axes.X;
							c.Height = 10;
							scalar.ValueBindable.BindTo( c.Current );
						} ),
						Vector2Action vec2 => vectorControl( 2 ).With( c => {
							var bindable = vec2.ValueBindable.GetBoundCopy();
							bindables.Add( bindable );
							bindable.BindValueChanged( v => {
								( (BasicSliderBar<float>)c[1] ).Current.Value = v.NewValue.X;
								( (BasicSliderBar<float>)c[2] ).Current.Value = v.NewValue.Y;
							} );
							( (BasicSliderBar<float>)c[1] ).Current.BindValueChanged( v => vec2.Value = vec2.Value with { X = v.NewValue } );
							( (BasicSliderBar<float>)c[2] ).Current.BindValueChanged( v => vec2.Value = vec2.Value with { Y = v.NewValue } );
						} ),
						Vector3Action vec3 => vectorControl( 3 ).With( c => {
							var bindable = vec3.ValueBindable.GetBoundCopy();
							bindables.Add( bindable );
							bindable.BindValueChanged( v => {
								( (BasicSliderBar<float>)c[1] ).Current.Value = v.NewValue.X;
								( (BasicSliderBar<float>)c[2] ).Current.Value = v.NewValue.Y;
								( (BasicSliderBar<float>)c[3] ).Current.Value = v.NewValue.Z;
							} );
							( (BasicSliderBar<float>)c[1] ).Current.BindValueChanged( v => vec3.Value = vec3.Value with { X = v.NewValue } );
							( (BasicSliderBar<float>)c[2] ).Current.BindValueChanged( v => vec3.Value = vec3.Value with { Y = v.NewValue } );
							( (BasicSliderBar<float>)c[3] ).Current.BindValueChanged( v => vec3.Value = vec3.Value with { Z = v.NewValue } );
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
		Bindable<Vector3> position = new();
		Bindable<Quaternion> rotation = new( Quaternion.Identity );
		public VirtualPoseAction ( object name, Bindable<Vector3> pos, Bindable<Quaternion> rot, Controller? source = null ) : base( name, source ) {
			position.BindTo( pos );
			rotation.BindTo( rot );
		}

		public override OpenVR.NET.Input.PoseInput? FetchData () {
			var pos = position.Value;
			var rot = rotation.Value;

			return new OpenVR.NET.Input.PoseInput {
				Position = new( pos.X, pos.Y, pos.Z ),
				Rotation = new( rot.X, rot.Y, rot.Z, rot.W )
			};
		}

		public override OpenVR.NET.Input.PoseInput? FetchDataForNextFrame () => FetchData();
		public override OpenVR.NET.Input.PoseInput? FetchDataForPrediction ( float secondsFromNow ) => FetchData();
	}
}
