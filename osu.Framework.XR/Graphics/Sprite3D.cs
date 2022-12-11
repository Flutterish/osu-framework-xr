using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics;

/// <summary>
/// A 3D quad which is always faced towards the camera
/// </summary>
public partial class Sprite3D : Model<BasicMesh> {
	public Sprite3D () {
		Mesh = BasicMesh.UnitQuad;
		Size = Vector2.One;
		base.OriginPosition = new( 0, -1 );
	}

	/// <summary>
	/// Controls the available space for the <see cref="Sprite3D"/> to fill. This defines the Fill Box size.
	/// It is <see cref="Vector2.One"/> by default
	/// </summary>
	public override Vector2 Size { 
		get => base.Size;
		set {
			base.Size = value;
			updateSize();
		}
	}

	/// <summary>
	/// Controls the available space for the <see cref="Sprite3D"/> to fill. This defines the Fill Box size.
	/// It is 1 by default
	/// </summary>
	public override float Width {
		get => base.Width;
		set {
			base.Width = value;
			updateSize();
		}
	}

	/// <summary>
	/// Controls the available space for the <see cref="Sprite3D"/> to fill. This defines the Fill Box size.
	/// It is 1 by default
	/// </summary>
	public override float Height {
		get => base.Height;
		set {
			base.Height = value;
			updateSize();
		}
	}

	Anchor origin = Anchor.BottomCentre;
	/// <summary>
	/// Controls which point of the <see cref="Sprite3D"/> is attached to the <see cref="Anchor"/>.
	/// It is <see cref="Anchor.BottomCentre"/> by default
	/// </summary>
	new public Anchor Origin {
		get => origin;
		set {
			origin = value;
			base.OriginPosition = new() {
				X = value.HasFlag( Anchor.x0 ) ? -1 : value.HasFlag( Anchor.x2 ) ? 1 : 0,
				Y = value.HasFlag( Anchor.y0 ) ? 1 : value.HasFlag( Anchor.y2 ) ? -1 : 0
			};
			updateAnchor();
		}
	}
	/// <summary>
	/// Controls which point of the <see cref="Sprite3D"/> is attached to the <see cref="Anchor"/> in range [-1;1]
	/// </summary>
	public override Vector2 OriginPosition {
		get => base.OriginPosition;
		set {
			base.OriginPosition = value;
			origin = Anchor.Custom;
			updateAnchor();
		}
	}

	Anchor anchor = Anchor.BottomCentre;
	/// <summary>
	/// Controls which point of the Fill Box <see cref="Origin"/> is attached to
	/// It is <see cref="Anchor.BottomCentre"/> by default
	/// </summary>
	new public Anchor Anchor {
		get => anchor;
		set {
			anchor = value;
			anchorPosition = new() {
				X = value.HasFlag( Anchor.x0 ) ? -1 : value.HasFlag( Anchor.x2 ) ? 1 : 0,
				Y = value.HasFlag( Anchor.y0 ) ? 1 : value.HasFlag( Anchor.y2 ) ? -1 : 0
			};
			updateAnchor();
		}
	}
	Vector2 anchorPosition = new( 0, -1 );
	/// <summary>
	/// Controls which point of the Fill Box <see cref="Origin"/> is attached to in range [-1;1]
	/// </summary>
	new public Vector2 AnchorPosition {
		get => anchorPosition;
		set {
			anchorPosition = value;
			anchor = Anchor.Custom;
			updateAnchor();
		}
	}

	Anchor fillBoxAnchor = Anchor.BottomCentre;
	/// <summary>
	/// Controls which point of the Fill Box is attached to the point in space defined by <see cref="Drawable3D.Position"/>.
	/// It is <see cref="Anchor.BottomCentre"/> by default
	/// </summary>
	public Anchor FillBoxAnchor {
		get => fillBoxAnchor;
		set {
			fillBoxAnchor = value;
			fillBoxAnchorPosition = new() {
				X = value.HasFlag( Anchor.x0 ) ? -1 : value.HasFlag( Anchor.x2 ) ? 1 : 0,
				Y = value.HasFlag( Anchor.y0 ) ? 1 : value.HasFlag( Anchor.y2 ) ? -1 : 0
			};
			updateAnchor();
		}
	}

	Vector2 fillBoxAnchorPosition = new( 0, -1 );
	/// <summary>
	/// Controls which point of the Fill Box is attached to the point in space defined by <see cref="Drawable3D.Position"/> in range [-1;1]
	/// </summary>
	public Vector2 FillBoxAnchorPosition {
		get => fillBoxAnchorPosition;
		set {
			fillBoxAnchorPosition = value;
			fillBoxAnchor = Anchor.Custom;
			updateAnchor();
		}
	}

	void updateAnchor () {
		Vector2 offset = (OriginPosition + Size * (fillBoxAnchorPosition - anchorPosition)) / 2;
		base.Origin = Vector3.Divide( new Vector3( offset.X, offset.Y, 0 ), Scale );
	}

	Axes autoSizeAxes = Axes.Both;
	/// <summary>
	/// Controls which <see cref="Axes"/> are automatically sized w.r.t. <see cref="FillAspectRatio"/> via <see cref="Drawable3D.Scale"/>.
	/// It is <see cref="Axes.Both"/> by default
	/// </summary>
	new public Axes AutoSizeAxes { 
		get => autoSizeAxes; 
		set {
			autoSizeAxes = value;
			updateSize();
		}
	}

	FillMode fillMode = FillMode.Fit;
	/// <summary>
	/// Controls the behavior of <see cref="AutoSizeAxes"/>
	/// </summary>
	new public FillMode FillMode {
		get => fillMode;
		set {
			fillMode = value;
			updateSize();
		}
	}

	bool automaticAspectRatio = true;
	/// <summary>
	/// Whether to infer <see cref="FillAspectRatio"/> from <see cref="Texture"/>.
	/// This will be set to <see langword="false"/> when <see cref="FillAspectRatio"/> is set manually
	/// </summary>
	public bool AutomaticAspectRatio {
		get => automaticAspectRatio;
		set {
			automaticAspectRatio = value;
			if ( value ) {
				fillAspectRatio = automaticFillAspectRatio;
				updateSize();
			}
		}
	}

	float automaticFillAspectRatio = 1;
	float fillAspectRatio = 1;
	/// <summary>
	/// The desired ratio of width to height when under the effect of a non-stretching <see cref="FillMode"/>
	/// </summary>
	new public float FillAspectRatio {
		get => fillAspectRatio;
		set {
			if ( fillAspectRatio == value ) return;

			if ( !float.IsFinite( value ) ) throw new ArgumentException( $@"{nameof( FillAspectRatio )} must be finite, but is {value}." );
			if ( value == 0 ) throw new ArgumentException( $@"{nameof( FillAspectRatio )} must be non-zero." );

			fillAspectRatio = value;
			automaticAspectRatio = false;

			updateSize();
		}
	}

	Texture? texture;
	/// <summary>
	/// The texture that this <see cref="Sprite3D"/> should draw.
	/// <see cref="Drawable.FillAspectRatio"/> is automatically set to the aspect ratio of the given texture
	/// </summary>
	public virtual Texture Texture {
		get => Material.Get<Texture>( "tex" ) ?? texture!;
		set {
			if ( value == texture )
				return;

			texture?.Dispose();
			texture = value;
			if ( Material != null )
				Material.SetTexture( "tex", value );

			automaticFillAspectRatio = (float)texture.Width / texture.Height;
			if ( AutomaticAspectRatio ) {
				fillAspectRatio = automaticFillAspectRatio;
				updateSize();
			}

			Invalidate( Invalidation.DrawNode );
		}
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		if ( texture != null )
			Material.SetTexture( "tex", texture );
	}

	void updateSize () {
		Vector2 size = this.Size;
		if ( size.X == 0 || size.Y == 0 ) { }
		else if ( fillMode == FillMode.Fit ) {
			var boxAspectRatio = size.X / size.Y;
			if ( fillAspectRatio > boxAspectRatio )
				size = new( size.X, size.X / fillAspectRatio );
			else
				size = new( size.Y * fillAspectRatio, size.Y );
		}
		else if ( fillMode == FillMode.Fill ) {
			var boxAspectRatio = size.X / size.Y;
			if ( fillAspectRatio < boxAspectRatio )
				size = new( size.X, size.X / fillAspectRatio );
			else
				size = new( size.Y * fillAspectRatio, size.Y );
		}

		if ( !autoSizeAxes.HasFlag( Axes.X ) )
			size.X = this.Size.X;
		if ( !autoSizeAxes.HasFlag( Axes.Y ) )
			size.Y = this.Size.Y;

		ScaleX = size.X;
		ScaleY = size.Y;

		updateAnchor();
	}

	bool faceCamera = true;
	public bool FaceCamera {
		get => faceCamera;
		set {
			faceCamera = value;
			Invalidate( Invalidation.DrawNode );
		}
	}

	protected override Material CreateDefaultMaterial ( MaterialStore materials )
		=> materials.GetNew( MaterialNames.Unlit );

	protected override MeshRendererDrawNode? CreateDrawNode3D ( int index )
		=> new SpriteDrawNode( this, index );

	protected class SpriteDrawNode : MeshRendererDrawNode {
		public SpriteDrawNode ( MeshRenderer<BasicMesh> source, int index ) : base( source, index ) { }

		Vector3 position;
		Matrix4 translation;
		Matrix4 scale;
		bool faceCamera;
		protected override void UpdateState () {
			base.UpdateState();
			faceCamera = ((Sprite3D)Source).faceCamera;
			if ( !faceCamera )
				return;

			position = Matrix.ExtractTranslation();
			translation = Matrix4.CreateTranslation( position );
			scale = Matrix4.CreateScale( Matrix.ExtractScale() );
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( Mesh is null )
				return;

			Bind();
			if ( faceCamera ) {
				var look = Matrix4.CreateFromQuaternion( ( renderer.ProjectionMatrix.ExtractCameraPosition() - position ).LookRotation() );
				Material.Shader.SetUniform( "mMatrix", scale * look * translation );
			}
			else {
				Material.Shader.SetUniform( "mMatrix", Matrix );
			}

			Mesh.Draw();
		}
	}
}
