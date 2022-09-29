using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics;

/// <summary>
/// A 3D drawable with a <see cref="BasicMesh"/> and a material
/// </summary>
public class BasicModel : Model<BasicMesh> {
	protected override BasicMesh CreateOwnMesh () {
		return new();
	}
}

/// <inheritdoc cref="Model{T}"/>
public class Model : Model<Mesh> { }

/// <summary>
/// A 3D drawable with a mesh and a material
/// </summary>
/// <typeparam name="T">The type of mesh</typeparam>
public class Model<T> : Drawable3D where T : Mesh {
	ulong materialMeshId = 1; // update thread mutable
	ulong linkedMaterialMeshId = 0; // 
	AttributeArray VAO = new();     // draw thread mutable

	T? mesh;
	bool ownMesh = false;
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
		}
	}

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
		return materials.GetNew( "unlit" );
	}

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		material ??= CreateDefaultMaterial( materials );
		if ( colour is Color4 color )
			material.SetIfDefault( "tint", color );
	}

	Color4? colour = null;
	new public Color4 Colour {
		get => Material?.Get<Color4>( "tint" ) ?? colour ?? Color4.White;
		set {
			if ( Colour == value )
				return;

			colour = value;
			Material?.Set( "tint", value );
			Invalidate( Invalidation.DrawNode );
		}
	}
	new public float Alpha {
		get => Colour.A;
		set => Colour = Colour with { A = value };
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
		Mesh mesh = null!;
		Material material = null!;
		protected Matrix4 Matrix;
		bool normalMatrixComputed;
		Matrix3 normalMatrix;
		ulong linkId;
		protected override void UpdateState () {
			mesh = Source.Mesh;
			material = Source.Material;
			Matrix = Source.Matrix;
			normalMatrixComputed = false;
			linkId = Source.materialMeshId;

			material.UpdateProperties( nodeIndex );
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( VAO.Bind() || linkId > Source.linkedMaterialMeshId ) {
				LinkAttributeArray( mesh, material );
				Source.linkedMaterialMeshId = linkId;
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
