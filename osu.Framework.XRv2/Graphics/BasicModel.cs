using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics;

public class BasicModel : Drawable3D {
	AttributeArray VAO = new();
	BasicMesh? mesh;
	bool ownMesh = false;
	public BasicMesh Mesh {
		get {
			if ( mesh is null ) {
				mesh = new();
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
		new protected BasicModel Source => (BasicModel)base.Source;
		int nodeIndex;
		public ModelDrawNode ( BasicModel source, int index ) : base( source ) {
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
