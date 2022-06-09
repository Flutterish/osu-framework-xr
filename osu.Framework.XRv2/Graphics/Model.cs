using osu.Framework.Graphics;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics;

public class Model : Drawable3D {
	AttributeArray VAO = new();
	Mesh? mesh;
	bool ownMesh = false;
	public Mesh Mesh {
		get {
			if ( mesh is null ) {
				mesh = new BasicMesh();
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

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		material ??= materials.GetNew( "unlit" );
	}

	Color4? colour = Color4.White;
	new public Color4 Colour {
		get => colour ?? ( material?.TryGet<Color4>( "tint", out var tint ) == true ? tint : Color4.White );
		set {
			if ( Colour == value )
				return;

			colour = value;
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
		new protected Model Source => (Model)base.Source;
		public ModelDrawNode ( Model source ) : base( source ) {
			VAO = source.VAO;
		}

		AttributeArray VAO = null!;
		Mesh mesh = null!;
		Material material = null!;
		Matrix4 matrix;
		Color4? tint;
		protected override void UpdateState () {
			mesh = Source.Mesh;
			material = Source.Material;
			matrix = Source.Matrix;
			tint = Source.colour;
			Source.colour = null;
		}

		public override void Draw ( object? ctx = null ) {
			if ( VAO.Bind() ) { // TODO check if mesh/material changed and update them
				LinkAttributeArray( mesh, material );
			}

			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
			material.Bind();
			if ( tint is Color4 color ) {
				material.TrySet( "tint", color );
				tint = null;
			}
			material.Shader.SetUniform( "mMatrix", ref matrix );
			mesh.Draw();
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
		}
	}
}
