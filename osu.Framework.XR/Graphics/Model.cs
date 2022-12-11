using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Physics;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics;

/// <summary>
/// A 3D drawable with a <see cref="BasicMesh"/> and a material. 
/// Implements a collider whose cache is invalidated every frame, but is disabled by default
/// </summary>
public partial class BasicModel : Model<BasicMesh>, IHasCollider {
	protected override BasicMesh CreateOwnMesh () {
		return new();
	}

	protected override void InvalidateMatrix () {
		base.InvalidateMatrix();
		ColliderMesh?.InvalidateMatrix();
	}

	protected override void Update () {
		base.Update();
		if ( ColliderMesh != null && IsColliderEnabled ) {
			ColliderMesh.Mesh = Mesh;
			ColliderMesh.InvalidateAll();
		}
	}

	protected TransformedBasicMesh? ColliderMesh { get; private set; }
	ITriangleMesh IHasCollider.ColliderMesh => ColliderMesh ??= new( Mesh, () => Matrix );
	public bool IsColliderEnabled { get; set; } = false;
	public ulong PhysicsLayer { get; set; } = 1;
}

/// <inheritdoc cref="Model{T}"/>
public partial class Model : Model<Mesh> { }

/// <summary>
/// A 3D drawable with a mesh and a material which renders a single mesh
/// </summary>
/// <typeparam name="T">The type of mesh</typeparam>
public partial class Model<T> : MeshRenderer<T> where T : Mesh {
	protected override Material CreateDefaultMaterial ( MaterialStore materials ) {
		return materials.GetNew( MaterialNames.Unlit );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		if ( colour is Color4 color )
			Material.SetIfDefault( "tint", color );
	}

	Color4? colour = null;
	override public ColourInfo Colour {
		get => Tint;
		set => Tint = value.TopLeft;
	}
	public Color4 Tint {
		get => Material?.Get<Color4>( "tint" ) ?? colour ?? Color4.White;
		set {
			if ( Tint == value )
				return;

			colour = value;
			Material?.Set( "tint", value );
			Invalidate( Invalidation.DrawNode );
		}
	}
	override public float Alpha {
		get => Tint.A;
		set => Tint = Tint with { A = value };
	}

	protected override MeshRendererDrawNode? CreateDrawNode3D ( int index )
		=> new ModelDrawNode( this, index );

	protected class ModelDrawNode : MeshRendererDrawNode {
		new protected Model<T> Source => (Model<T>)base.Source;
		public ModelDrawNode ( Model<T> source, int index ) : base( source, index ) { }

		bool normalMatrixComputed;
		Matrix3 normalMatrix;

		protected override void UpdateState () {
			base.UpdateState();
			normalMatrixComputed = false;
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( Mesh is null )
				return;

			Bind();

			Material.Shader.SetUniform( "mMatrix", ref Matrix );
			if ( Material.Shader.TryGetUniform<Matrix3>( "mNormal", out var mNormal ) ) {
				if ( !normalMatrixComputed ) {
					var mat = Matrix.Inverted();
					mat.Transpose();
					normalMatrix = new Matrix3( mat );
					normalMatrixComputed = true;
				}

				mNormal.UpdateValue( ref normalMatrix );
			}
			Mesh.Draw();
		}
	}
}
