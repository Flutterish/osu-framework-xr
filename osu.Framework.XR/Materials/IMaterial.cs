using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Shaders;
using osu.Framework.XR.Components;
using System;
using System.Collections.Generic;

namespace osu.Framework.XR.Materials {
	public interface IMaterial {
		string ResourceName { get; }
		IShader Shader { get; }
		void Bind ( Drawable3D.DrawNode3D.DrawSettings settings );
		void Unbind ();
		bool IsLoaded { get; }

		MaterialUniform<T> GetUniform<T> ( string name ) where T : struct, IEquatable<T>;
		IMaterialUniform[] GetAllUniforms ();

		public TextureGL MainTexture { get; set; }
		public List<TextureGL> AllTextures { get; }
	}
}
