using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.XR.Input;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Rendering;
using osuTK;
using System.Linq;
using static osu.Framework.XR.Components.Drawable3D.DrawNode3D;

namespace osu.Framework.XR.Components {
	/// <summary>
	/// A 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public abstract partial class Panel : Model, IHasCollider {
		public PhysicsLayer PhysicsLayer { get; set; } = PhysicsLayer.All;
		public readonly VirtualInputManager EmulatedInput = new();
		private PlatformActionContainer platformActions = new();
		public Container<Drawable> Source => tooltips;
		/// <summary>
		/// Non-stretching scaling applied to the content
		/// </summary>
		public Bindable<Vector2> ContentScale = new( Vector2.One );
		public BufferedCapture SourceCapture { get; } = new();
		protected bool IsMeshInvalidated = true;

		/// <summary>
		/// Makes the content use the 2D height of this panel and its own width.
		/// </summary>
		public Panel AutosizeX () {
			EmulatedInput.RelativeSizeAxes = Axes.Y;
			SourceCapture.RelativeSizeAxes = Axes.Y;
			platformActions.RelativeSizeAxes = Axes.Y;
			tooltips.RelativeSizeAxes = Axes.Y;

			EmulatedInput.AutoSizeAxes = Axes.X;
			SourceCapture.AutoSizeAxes = Axes.X;
			platformActions.AutoSizeAxes = Axes.X;
			tooltips.AutoSizeAxes = Axes.X;

			return this;
		}
		/// <summary>
		/// Makes the content use the 2D width of this panel and its own height.
		/// </summary>
		public Panel AutosizeY () {
			EmulatedInput.RelativeSizeAxes = Axes.X;
			SourceCapture.RelativeSizeAxes = Axes.X;
			platformActions.RelativeSizeAxes = Axes.X;
			tooltips.RelativeSizeAxes = Axes.X;

			EmulatedInput.AutoSizeAxes = Axes.Y;
			SourceCapture.AutoSizeAxes = Axes.Y;
			platformActions.AutoSizeAxes = Axes.Y;
			tooltips.AutoSizeAxes = Axes.Y;

			return this;
		}
		/// <summary>
		/// Makes the content use the its own width and height.
		/// </summary>
		public Panel AutosizeBoth () {
			EmulatedInput.RelativeSizeAxes = Axes.None;
			SourceCapture.RelativeSizeAxes = Axes.None;
			platformActions.RelativeSizeAxes = Axes.None;
			tooltips.RelativeSizeAxes = Axes.None;

			EmulatedInput.AutoSizeAxes = Axes.Both;
			SourceCapture.AutoSizeAxes = Axes.Both;
			platformActions.AutoSizeAxes = Axes.Both;
			tooltips.AutoSizeAxes = Axes.Both;

			return this;
		}
		/// <summary>
		/// Makes the content use the 2D width and height of this panel.
		/// </summary>
		public Panel AutosizeNone () {
			EmulatedInput.RelativeSizeAxes = Axes.Both;
			SourceCapture.RelativeSizeAxes = Axes.Both;
			platformActions.RelativeSizeAxes = Axes.Both;
			tooltips.RelativeSizeAxes = Axes.Both;

			EmulatedInput.AutoSizeAxes = Axes.None;
			SourceCapture.AutoSizeAxes = Axes.None;
			platformActions.AutoSizeAxes = Axes.None;
			tooltips.AutoSizeAxes = Axes.None;

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
			platformActions.Add( tooltips = CreateTooltipContainer() ?? new Container() );
			AddDrawable( SourceCapture );

			ShouldBeDepthSorted = true;

			AutosizeNone();
		}

		Container<Drawable> tooltips;
		protected virtual TooltipContainer? CreateTooltipContainer () => null;
		protected abstract void RecalculateMesh ();

		/// <summary>
		/// The texture position from top left.
		/// </summary>
		public Vector2 TexturePositionAt ( int trisIndex, Vector3 position ) {
			var face = Faces[ trisIndex ];
			var barycentric = Triangles.Barycentric( face, position );
			var tris = Mesh.Tris[ trisIndex ];
			var textureCoord =
				  Mesh.TextureCoordinates[ (int)tris.A ] * barycentric.X
				+ Mesh.TextureCoordinates[ (int)tris.B ] * barycentric.Y
				+ Mesh.TextureCoordinates[ (int)tris.C ] * barycentric.Z;
			return new Vector2( MainTexture.Width * textureCoord.X, MainTexture.Height * ( 1 - textureCoord.Y ) );
		}

		private Vector2 lastTextureSize;
		public override void BeforeDraw ( DrawSettings settings ) {
			if ( SourceCapture is null ) return;
			if ( SourceCapture.Capture is null ) return;
			MainTexture = SourceCapture.Capture;
			if ( MainTexture.Size != lastTextureSize ) {
				IsMeshInvalidated = true;
				lastTextureSize = MainTexture.Size;
			}
		}

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
	}
}
