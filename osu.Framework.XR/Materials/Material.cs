using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Components;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace osu.Framework.XR.Materials {
	public class Material : IMaterial {
		public IShader Shader { get; private set; }
		public string ResourceName { get; private set; }
		public readonly string Name;

		private Dictionary<string, IMaterialUniform> uniformNames = new();
		private IMaterialUniform[]? uniforms;

		public TextureGL MainTexture {
			get => AllTextures[ 0 ];
			set => AllTextures[ 0 ] = value;
		}
		public List<TextureGL> AllTextures { get; } = new();
		private static TextureGL createWhilePixel () { // this exists because the default white pixel is for some reason a gradient from white to transparent
			var upload = new TextureUpload( new Image<Rgba32>( 1, 1, new Rgba32( 1f, 1f, 1f, 1f ) ) );
			var txt = new Texture( 1, 1 );
			txt.SetData( upload );

			return txt.TextureGL;
		}
		public static TextureGL WhitePixelTexture { get; } = createWhilePixel();

		internal Material ( IShader shader, string resourceName, string name ) {
			Shader = shader;
			Name = name;
			ResourceName = resourceName;
			AllTextures.Add( WhitePixelTexture );
		}

		public bool IsLoaded => Shader.IsLoaded;

		private bool isBound = false;
		public void Bind ( Drawable3D.DrawNode3D.DrawSettings settings ) {
			if ( !IsLoaded || isBound ) return;

			if ( uniforms is null ) {
				uniforms = Shader.GetAllUniforms();
				for ( int i = 0; i < uniforms.Length; i++ ) {
					uniformNames.Add( uniforms[ i ].Name, uniforms[ i ] );
				}
			}

			for ( int i = 0; i < uniforms.Length; i++ ) {
				uniforms[ i ].Update();
			}

			Shader.Bind();
			isBound = true;
			GlobalMaterialUniformManager.Bind( this, settings );
			for ( int i = 0; i < AllTextures.Count; i++ ) {
				if ( !AllTextures[ i ].Bind( osuTK.Graphics.ES30.TextureUnit.Texture0 + i ) ) return;
			}
		}

		public void Unbind () {
			Shader.Unbind();
			isBound = false;
		}

		public MaterialUniform<T> GetUniform<T> ( string name ) where T : struct, IEquatable<T>
			=> (MaterialUniform<T>)uniformNames[ name ];

		public IMaterialUniform[] GetAllUniforms ()
			=> uniforms!;
	}
}
