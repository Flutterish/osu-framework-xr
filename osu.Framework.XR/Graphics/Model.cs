using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Physics;
using osuTK.Graphics;
using System.Diagnostics.CodeAnalysis;

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
		colliderMesh?.InvalidateMatrix();
	}

	protected override void Update () {
		base.Update();
		if ( colliderMesh != null ) {
			colliderMesh.Mesh = Mesh;
			colliderMesh.InvalidateAll();
		}
	}

	TransformedBasicMesh? colliderMesh;
	public ITriangleMesh ColliderMesh => colliderMesh ??= new( Mesh, () => Matrix );
	public bool IsColliderEnabled { get; set; } = false;
	public ulong PhysicsLayer { get; set; } = 1;
}

/// <inheritdoc cref="Model{T}"/>
public partial class Model : Model<Mesh> { }

/// <summary>
/// A 3D drawable with a mesh and a material
/// </summary>
/// <typeparam name="T">The type of mesh</typeparam>
public partial class Model<T> : Drawable3D where T : Mesh {
	// shared data
	ulong materialMeshId = 1;
	ulong linkedMaterialMeshId = 0; 
	ulong meshId = 0;
	ulong linkedMeshId = 0;
	AttributeArray VAO = new();

	T? mesh;
	bool ownMesh = false;
	[AllowNull]
	public T Mesh {
		get {
			if ( mesh is null ) {
				mesh = CreateOwnMesh();
				ownMesh = true;
			}
			return mesh;
		}
		set {
			if ( ownMesh )
				mesh!.Dispose();

			if ( mesh?.Descriptor != value?.Descriptor )
				materialMeshId++;
			mesh = value;
			ownMesh = false;
			meshId++;
			Invalidate( Invalidation.DrawNode );
		}
	}

	/// <summary>
	/// Creates a default mesh when <see cref="Mesh"/> is accessed but not set
	/// </summary>
	protected virtual T CreateOwnMesh () {
		throw new InvalidOperationException( $"{GetType().ReadableName()} can not create its own mesh" );
	}

	Material? material;
	public Material Material {
		get => material!;
		set {
			material = value;
			if ( material.Shader != value.Shader )
				materialMeshId++;
			Invalidate( Invalidation.DrawNode );
		}
	}

	protected virtual Material CreateDefaultMaterial ( MaterialStore materials ) {
		return materials.GetNew( MaterialNames.Unlit );
	}

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		material ??= CreateDefaultMaterial( materials );
		if ( colour is Color4 color )
			material.SetIfDefault( "tint", color );
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

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			if ( ownMesh )
				mesh!.Dispose();
			VAO.Dispose();
		}

		base.Dispose( isDisposing );
	}

	protected override ModelDrawNode? CreateDrawNode3D ( int index )
		=> new ModelDrawNode( this, index );

	protected class ModelDrawNode : DrawNode3D {
		new protected Model<T> Source => (Model<T>)base.Source;
		int nodeIndex;
		public ModelDrawNode ( Model<T> source, int index ) : base( source ) {
			nodeIndex = index;
			VAO = source.VAO;
		}

		AttributeArray VAO = null!;
		Mesh? mesh;
		Material material = null!;
		protected Matrix4 Matrix;
		bool normalMatrixComputed;
		Matrix3 normalMatrix;

		ulong linkId;
		ulong meshId;
		protected override void UpdateState () {
			mesh = Source.mesh;
			material = Source.Material;
			Matrix = Source.Matrix;
			normalMatrixComputed = false;
			linkId = Source.materialMeshId;
			meshId = Source.meshId;

			material.UpdateProperties( nodeIndex );
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( mesh is null )
				return;

			if ( linkId > Source.linkedMaterialMeshId ) {
				VAO.Clear();
				Source.linkedMaterialMeshId = linkId;
			}

			if ( VAO.Bind() || meshId > Source.linkedMeshId ) {
				LinkAttributeArray( mesh, material );
				Source.linkedMeshId = meshId;
			}

			material.Bind( nodeIndex );
			material.Shader.SetUniform( "mMatrix", ref Matrix );
			if ( material.Shader.TryGetUniform<Matrix3>( "mNormal", out var mNormal ) ) {
				if ( !normalMatrixComputed ) {
					var mat = Matrix.Inverted();
					mat.Transpose();
					normalMatrix = new Matrix3( mat );
					normalMatrixComputed = true;
				}

				mNormal.UpdateValue( ref normalMatrix );
			}
			mesh.Draw();
		}
	}
}
