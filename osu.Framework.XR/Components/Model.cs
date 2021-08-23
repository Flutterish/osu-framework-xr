using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace osu.Framework.XR.Components {
	/// <summary>
	/// A <see cref="Drawable3D"/> which renders a <see cref="Graphics.Mesh"/>.
	/// </summary>
	public class Model : Drawable3D {
		public bool IsVisible = true;
		public Mesh Mesh { get; set; } = new();
		public bool UseGammaCorrection = false;
		public TextureGL MainTexture {
			get => AllTextures[ 0 ];
			set => AllTextures[ 0 ] = value;
		}
		public readonly List<TextureGL> AllTextures = new();
		private static TextureGL createWhilePixel () { // this exists because the default white pixel is for some reason a gradient from white to transparent
			var upload = new TextureUpload( new Image<Rgba32>( 1, 1, new Rgba32( 1f, 1f, 1f, 1f ) ) );
			var txt = new Texture( 1, 1 );
			txt.SetData( upload );

			return txt.TextureGL;
		}
		public static TextureGL WhitePixel { get; } = createWhilePixel();
		public Model () {
			Faces = new( i => Transform.Matrix * Mesh.Faces[ i ] );
			AllTextures.Add( WhitePixel );
		}
		protected override DrawNode3D CreateDrawNode ()
			=> new ModelDrawNode( this );

		public readonly CachedReadonlyIndexer<int, Face> Faces;

		protected override void Update () {
			base.Update();
			Faces.ClearCache();
		}

		public override Vector3 Size { get => Mesh.BoundingBox.Size; set => throw new InvalidOperationException( $"Cannot set size of a {nameof(Model)}." ); }
		public override Vector3 Centre => Mesh.BoundingBox.Min + Mesh.BoundingBox.Size / 2;

		public Color4 Tint {
			get => base.Colour;
			set => base.Colour = value;
		}
	}
	public class ModelDrawNode : ModelDrawNode<Model> {
		public ModelDrawNode ( Model source ) : base( source ) { }
	}
	public class ModelDrawNode<T> : Drawable3D.XrObjectDrawNode<T> where T : Model {
		public ModelDrawNode ( T source ) : base( source ) { }
		protected virtual Mesh GetMesh () => Source.Mesh;

		private bool notInitialized = true;
		private Mesh? mesh;
		private ulong lastUpdateVersion;

		public override void Draw ( DrawSettings settings ) {
			if ( !Source.IsVisible ) return;

			var newMesh = GetMesh();
			if ( mesh != newMesh ) {
				mesh = newMesh;
				lastUpdateVersion = 0;
			}

			if ( !Shaders.Shader3D.IsLoaded ) return;
			if ( mesh is null ) return;

			if ( notInitialized ) {
				Initialize();
				notInitialized = false;
			}

			if ( lastUpdateVersion != mesh.UpdateVersion && mesh.IsReady ) {
				UpdateMesh( mesh );
				lastUpdateVersion = mesh.UpdateVersion;
			}

			for ( int i = 0; i < Source.AllTextures.Count; i++ ) {
				if ( !Source.AllTextures[ i ].Bind( osuTK.Graphics.ES30.TextureUnit.Texture0 + i ) ) return;
			}

			Shaders.Shader3D.Bind();
			GL.BindVertexArray( VAO );

			var a = settings.WorldToCamera;
			var b = settings.CameraToClip;
			var c = (Matrix4)Transform.Matrix;

			GL.UniformMatrix4( worldToCamera ??= GL.GetUniformLocation( Shaders.Shader3D, Shaders.VERTEX_3D.WorldToCameraMatrix ), true, ref a );
			GL.UniformMatrix4( cameraToClip ??= GL.GetUniformLocation( Shaders.Shader3D, Shaders.VERTEX_3D.CameraToClipMatrix ), true, ref b );
			GL.UniformMatrix4( localToWorld ??= GL.GetUniformLocation( Shaders.Shader3D, Shaders.VERTEX_3D.LocalToWorldMatrix ), true, ref c );
			GL.Uniform1( useGamma ??= GL.GetUniformLocation( Shaders.Shader3D, Shaders.FRAGMENT_3D.UseGammaCorrection ), Source.UseGammaCorrection ? 1 : 0 );
			GL.Uniform4( tint ??= GL.GetUniformLocation( Shaders.Shader3D, Shaders.FRAGMENT_3D.Tint ), new Color4( Source.Tint.R, Source.Tint.G, Source.Tint.B, Source.Tint.A * Source.Alpha ) );
			GL.DrawElements( PrimitiveType.Triangles, indiceCount, DrawElementsType.UnsignedInt, 0 );
			GL.BindVertexArray( 0 );
			Shaders.Shader3D.Unbind();
		}

		private static int? worldToCamera; // TODO would be nice to get rid of the nullcheck
		private static int? cameraToClip;
		private static int? localToWorld;
		private static int? useGamma;
		private static int? tint;

		private int VAO;
		private int buffer;
		private int EBO;
		private int indiceCount;
		protected void Initialize () {
			VAO = GL.GenVertexArray();
			buffer = GL.GenBuffer();
			EBO = GL.GenBuffer();
		}
		protected void UpdateMesh ( Mesh mesh ) {
			GL.BindVertexArray( VAO );
			indiceCount = mesh.UploadToGPU( attribLocation( "vertex" ), attribLocation( "UV" ), buffer, EBO );
			GL.BindVertexArray( 0 );
		}

		public override void Dispose () {
			base.Dispose();
			GL.DeleteVertexArray( VAO );
			GL.DeleteBuffer( buffer );
			GL.DeleteBuffer( EBO );
		}

		private static Dictionary<string, int> attribs = new();
		private int attribLocation ( string name ) {
			if ( attribs.TryGetValue( name, out var handle ) ) return handle;
			handle = GL.GetAttribLocation( Shaders.Shader3D, name );
			attribs.Add( name, handle );
			return handle;
		}
	}
}
