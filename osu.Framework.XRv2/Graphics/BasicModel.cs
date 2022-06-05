using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
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

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		if ( material is null ) {
			material = materials.GetNew( "unlit" );
			material.CreateUpload( m => {
				m.Set( "tex", Texture.WhitePixel );
				m.Set( "tint", Colour );
				m.Set( "subImage", Texture.WhitePixel.GetTextureRect() );
			} ).Enqueue();
		}
	}

	Color4? colour = Color4.White;
	new public Color4 Colour {
		get => colour ?? (material?.IsLoaded == true ? material.Get<Color4>( "tint" ) : Color4.White);
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
		new protected BasicModel Source => (BasicModel)base.Source;
		public ModelDrawNode ( BasicModel source ) : base( source ) {
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
				mesh.ElementBuffer!.Bind();
				mesh.VertexBuffers[0].Link( material.Shader, stackalloc int[] { material.Shader.GetAttrib( "aPos" ), material.Shader.GetAttrib( "aUv" ) } );
			}

			material.Bind();
			if ( tint is Color4 color ) {
				material.Set( "tint", color );
				tint = null;
			}
			material.Shader.SetUniform( "mMatrix", ref matrix );
			material.Shader.SetUniform( "gProj", ( (BasicDrawContext)ctx! ).ProjectionMatrix ); // TODO extract this
			mesh.Draw();
		}
	}
}
