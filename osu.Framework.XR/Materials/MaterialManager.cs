using osu.Framework.Graphics.Shaders;
using osu.Framework.IO.Stores;
using System.Collections.Generic;

namespace osu.Framework.XR.Materials {
	public class MaterialManager {
		private Dictionary<string, Dictionary<string, IMaterial>> sharedMaterials = new();

		private readonly ShaderManager shaderManager;
		public MaterialManager ( IResourceStore<byte[]> store ) {
			shaderManager = new( store );
		}

		/// <summary>
		/// Retrives a shared instance of a material or creates it if it doesnt exist. The shared material will have the same settings on all instances with the same name.
		/// </summary>
		public IMaterial LoadShared ( string fileName, string sharedName ) {
			if ( !sharedMaterials.TryGetValue( fileName, out var shared ) ) {
				sharedMaterials.Add( fileName, shared = new() );
			}

			if ( !shared.TryGetValue( sharedName, out var mat ) ) {
				shared.Add( sharedName, mat = LoadNew( fileName, sharedName ) );
			}

			return mat;
		}

		/// <summary>
		/// Creates a completely new material instance
		/// </summary>
		public IMaterial LoadNew ( string fileName, string? name = null ) {
			var shader = shaderManager.Load( fileName, fileName );
			return new Material( shader, fileName, name ?? fileName );
		}
	}
}
