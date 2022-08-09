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
	}

	Color4? colour = null;
	new public Color4 Colour {
		get => colour ?? ( material?.IsLoaded == true ? material.Get<Color4>( "tint" ) : Color4.White );
		set {
			if ( Colour == value )
				return;

			colour = value;
			Invalidate( Invalidation.DrawNode );
		}
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			if ( ownMesh )
				mesh!.Dispose();
			VAO.Dispose();
		}

		base.Dispose( isDisposing );
	}

	protected override DrawNode3D? CreateDrawNode3D ()
		=> new ModelDrawNode( this );

	class ModelDrawNode : DrawNode3D {
		new protected BasicModel Source => (BasicModel)base.Source;
		public ModelDrawNode ( BasicModel source ) : base( source ) {
			VAO = source.VAO;
		}

		AttributeArray VAO = null!;
		Mesh mesh = null!;
		Material material = null!;
		Matrix4 matrix;
		bool normalMatrixComputed;
		Matrix3 normalMatrix;
		Color4? tint;
		protected override void UpdateState () {
			mesh = Source.Mesh;
			material = Source.Material;
			matrix = Source.Matrix;
			tint = Source.colour;
			Source.colour = null;
			normalMatrixComputed = false;
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( VAO.Bind() ) { // TODO check if mesh/material changed and update them
				LinkAttributeArray( mesh, material );
			}

			material.Bind();
			if ( tint is Color4 color ) {
				material.TrySet( "tint", color );
				tint = null;
			}
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
