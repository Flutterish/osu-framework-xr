using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Framework.XR.Input;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Rendering;
using osuTK;
using System.Linq;

namespace osu.Framework.XR.Components {
	/// <summary>
	/// A 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	[Cached(type: typeof(ISafeArea))]
	public abstract partial class Panel : Model, IHasCollider, ISafeArea, IDrawable {
		public PhysicsLayer PhysicsLayer { get; set; } = PhysicsLayer.All;
		public readonly VirtualInputManager EmulatedInput = new();
		private PlatformActionContainer platformActions = new();
		public Container<Drawable> Source => contentWrapper;
		/// <summary>
		/// Non-stretching scaling applied to the content
		/// </summary>
		public Bindable<Vector2> ContentScale = new( Vector2.One );
		public BufferedCapture SourceCapture { get; } = new();
		protected bool IsMeshInvalidated = true;

		// this ensures that the panel is the "root node" for cases like buffered containers which clip their size to the root node
		CompositeDrawable IDrawable.Parent => null!;

		// TODO remove the autosizes
		/// <summary>
		/// Makes the content use the 2D height of this panel and its own width.
		/// </summary>
		public Panel AutosizeX () {
			EmulatedInput.RelativeSizeAxes = Axes.Y;
			SourceCapture.RelativeSizeAxes = Axes.Y;
			platformActions.RelativeSizeAxes = Axes.Y;
			contentWrapper.RelativeSizeAxes = Axes.Y;

			EmulatedInput.AutoSizeAxes = Axes.X;
			SourceCapture.AutoSizeAxes = Axes.X;
			platformActions.AutoSizeAxes = Axes.X;
			contentWrapper.AutoSizeAxes = Axes.X;

			return this;
		}
		/// <summary>
		/// Makes the content use the 2D width of this panel and its own height.
		/// </summary>
		public Panel AutosizeY () {
			EmulatedInput.RelativeSizeAxes = Axes.X;
			SourceCapture.RelativeSizeAxes = Axes.X;
			platformActions.RelativeSizeAxes = Axes.X;
			contentWrapper.RelativeSizeAxes = Axes.X;

			EmulatedInput.AutoSizeAxes = Axes.Y;
			SourceCapture.AutoSizeAxes = Axes.Y;
			platformActions.AutoSizeAxes = Axes.Y;
			contentWrapper.AutoSizeAxes = Axes.Y;

			return this;
		}
		/// <summary>
		/// Makes the content use the its own width and height.
		/// </summary>
		public Panel AutosizeBoth () {
			EmulatedInput.RelativeSizeAxes = Axes.None;
			SourceCapture.RelativeSizeAxes = Axes.None;
			platformActions.RelativeSizeAxes = Axes.None;
			contentWrapper.RelativeSizeAxes = Axes.None;

			EmulatedInput.AutoSizeAxes = Axes.Both;
			SourceCapture.AutoSizeAxes = Axes.Both;
			platformActions.AutoSizeAxes = Axes.Both;
			contentWrapper.AutoSizeAxes = Axes.Both;

			return this;
		}
		/// <summary>
		/// Makes the content use the 2D width and height of this panel.
		/// </summary>
		public Panel AutosizeNone () {
			EmulatedInput.RelativeSizeAxes = Axes.Both;
			SourceCapture.RelativeSizeAxes = Axes.Both;
			platformActions.RelativeSizeAxes = Axes.Both;
			contentWrapper.RelativeSizeAxes = Axes.Both;

			EmulatedInput.AutoSizeAxes = Axes.None;
			SourceCapture.AutoSizeAxes = Axes.None;
			platformActions.AutoSizeAxes = Axes.None;
			contentWrapper.AutoSizeAxes = Axes.None;

			return this;
		}

		public Panel () {
			UseGammaCorrection = true;

			ContentScale.ValueChanged += v => {
				SourceCapture.Size = v.NewValue;
				IsMeshInvalidated = true;
			};

			SourceCapture.Add( EmulatedInput );
			EmulatedInput.Add( platformActions );
			platformActions.Add( contentWrapper = CreateContentWrapper() );
			AddDrawable( SourceCapture );

			ShouldBeDepthSorted = true;

			AutosizeNone();
		}

		Container<Drawable> contentWrapper;
		protected virtual Container CreateContentWrapper () => new Container();
		protected abstract void RecalculateMesh ();

		/// <summary>
		/// The texture position from top left.
		/// </summary>
		public Vector2 TexturePositionAt ( int trisIndex, Vector3 position ) {
			var face = Faces[ trisIndex ];
			var barycentric = Triangles.BarycentricFast( face, position );
			var tris = Mesh.Tris[ trisIndex ];
			var textureCoord =
				  Mesh.TextureCoordinates[ (int)tris.A ] * barycentric.X
				+ Mesh.TextureCoordinates[ (int)tris.B ] * barycentric.Y
				+ Mesh.TextureCoordinates[ (int)tris.C ] * barycentric.Z;
			return new Vector2( MainTexture.Width * textureCoord.X, MainTexture.Height * ( 1 - textureCoord.Y ) );
		}

		private Vector2 lastTextureSize;

		protected override void Update () {
			base.Update();
			if ( IsMeshInvalidated ) {
				Mesh.IsReady = false;
				RecalculateMesh();
				Mesh.IsReady = true;
				IsMeshInvalidated = false;
			}
		}

		public virtual bool IsColliderEnabled => Source.Any( x => x.IsPresent );
		public override void Show () {
			this.FadeIn( 300, Easing.Out );
			foreach ( var i in Source ) {
				i.Show();
			}
		}
		public override void Hide () {
			this.FadeOut( 300, Easing.Out ).Then().Schedule( () => {
				foreach ( var i in Source ) {
					i.Hide();
				}
			} );
		}

		protected override DrawNode3D CreateDrawNode ()
			=> new PanelDrawNode( this );

		class PanelDrawNode : ModelDrawNode<Panel> {
			public PanelDrawNode ( Panel source ) : base( source ) {
			}

			public override void Draw ( DrawSettings settings ) {
				if ( Source.SourceCapture?.Capture is not null ) {
					Source.MainTexture = Source.SourceCapture.Capture;
					if ( Source.MainTexture.Size != Source.lastTextureSize ) {
						Source.IsMeshInvalidated = true;
						Source.lastTextureSize = Source.MainTexture.Size;
					}
				}
				base.Draw( settings );
			}
		}

		public Framework.Graphics.Primitives.Quad ExpandRectangleToSpaceOfOtherDrawable ( IDrawable other ) {
			return ToSpaceOfOtherDrawable( DrawRectangle, other );
		}

		private readonly BindableSafeArea safeArea = new BindableSafeArea();
		public RectangleF AvailableNonSafeSpace => DrawRectangle;
		public BindableSafeArea SafeAreaPadding => safeArea;
		EdgeEffectParameters IContainer.EdgeEffect { get => EdgeEffect; set => EdgeEffect = value; }
		Vector2 IContainer.RelativeChildSize { get => RelativeChildSize; set => RelativeChildSize = value; }
		Vector2 IContainer.RelativeChildOffset { get => RelativeChildOffset; set => RelativeChildOffset = value; }
	}
}
