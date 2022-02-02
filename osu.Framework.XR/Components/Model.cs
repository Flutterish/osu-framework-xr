using osu.Framework.Allocation;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Shaders;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Materials;
using osu.Framework.XR.Materials.Builtin;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
		public List<TextureGL> AllTextures => unlitMaterial?.AllTextures ?? (textures ??= new() { Material.WhitePixelTexture } );
		public Model () {
			Faces = new( i => Transform.Matrix * Mesh.Faces[ i ] );
		}
		protected override DrawNode3D CreateDrawNode ()
			=> new ModelDrawNode( this );

		public readonly CachedReadonlyIndexer<int, Face> Faces;

		protected override void Update () {
			base.Update();
			Faces.ClearCache();
		}

		private List<TextureGL>? textures;
		[BackgroundDependencyLoader]
		private void load ( MaterialManager materials ) {
			unlitMaterial = materials.LoadNew( UnlitMaterialDescriptor.Name );
			if ( textures != null ) {
				unlitMaterial.AllTextures.AddRange( textures );
				textures = null;
			}
		}
		[MaybeNull, NotNull]
		private IMaterial unlitMaterial;

		public override Vector3 Size { get => Mesh.BoundingBox.Size; set => throw new InvalidOperationException( $"Cannot set size of a {nameof(Model)}." ); }
		public override Vector3 Centre => Mesh.BoundingBox.Min + Mesh.BoundingBox.Size / 2;

		public Color4 Tint {
			get => Colour;
			set => Colour = value;
		}

		public class ModelDrawNode : ModelDrawNode<Model> {
			public ModelDrawNode ( Model source ) : base( source ) { }
		}
		public class ModelDrawNode<T> : XrObjectDrawNode<T> where T : Model {
			public ModelDrawNode ( T source ) : base( source ) {
				material = source.unlitMaterial;
			}
			protected virtual Mesh GetMesh () => Source.Mesh;
			private IMaterial material;

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

				if ( !material.IsLoaded ) return;
				if ( mesh is null ) return;

				if ( notInitialized ) {
					Initialize();
					notInitialized = false;
				}

				if ( lastUpdateVersion != mesh.UpdateVersion && mesh.IsReady ) {
					lastUpdateVersion = mesh.UpdateVersion;
					UpdateMesh( mesh );
				}

				material.Bind( settings );
				
				GL.BindVertexArray( VAO );

				material.GetUniform<Matrix4>( UnlitMaterialDescriptor.LocalToWorldMatrix ).UpdateValue( Transform.Matrix.Transposed );
				material.GetUniform<bool>( UnlitMaterialDescriptor.UseGammaCorrection ).UpdateValue( ref Source.UseGammaCorrection );
				material.GetUniform<Vector4>( UnlitMaterialDescriptor.Tint ).UpdateValue( new Vector4( Source.Tint.R, Source.Tint.G, Source.Tint.B, Source.Tint.A * Source.Alpha ) );

				GL.DrawElements( PrimitiveType.Triangles, indiceCount, DrawElementsType.UnsignedInt, 0 ); // TODO draw batches of materials rather than models
				GL.BindVertexArray( 0 );

				material.Unbind();
			}

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
				handle = GL.GetAttribLocation( (Shader)material.Shader, name );
				attribs.Add( name, handle );
				return handle;
			}
		}
	}
}
