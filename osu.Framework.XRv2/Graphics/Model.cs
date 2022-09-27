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
	AttributeArray VAO = new();
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

			mesh = value;
			ownMesh = false;
		}
	}

	protected virtual T CreateOwnMesh () {
		throw new InvalidOperationException( $"This implementation of {nameof(Model<T>)} can not create its own mesh" );
	}

	Material? material;
	public Material Material {
		get => material!;
		set {
			material = value;
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
			material.Set( "tint", color );
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

	protected override DrawNode3D? CreateDrawNode3D ( int index )
		=> new ModelDrawNode( this, index );

	class ModelDrawNode : DrawNode3D {
		new protected Model<T> Source => (Model<T>)base.Source;
		int nodeIndex;
		public ModelDrawNode ( Model<T> source, int index ) : base( source ) {
			nodeIndex = index;
			VAO = source.VAO;
		}

		AttributeArray VAO = null!;
		Mesh mesh = null!;
		Material material = null!;
		Matrix4 matrix;
		bool normalMatrixComputed;
		Matrix3 normalMatrix;
		protected override void UpdateState () {
			mesh = Source.Mesh;
			material = Source.Material;
			matrix = Source.Matrix;
			normalMatrixComputed = false;

			material.UpdateProperties( nodeIndex );
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( VAO.Bind() ) { // TODO check if mesh/material changed and update them
				LinkAttributeArray( mesh, material );
			}

			material.Bind( nodeIndex );
			material.Shader.SetUniform( "mMatrix", ref matrix );
			if ( material.Shader.TryGetUniform<Matrix3>( "mNormal", out var mNormal ) ) {
				if ( !normalMatrixComputed ) {
					var mat = matrix.Inverted();
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
