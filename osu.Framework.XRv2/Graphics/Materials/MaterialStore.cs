using osu.Framework.IO.Stores;
using osu.Framework.XR.Graphics.Shaders;

namespace osu.Framework.XR.Graphics.Materials;

public class MaterialStore {
	IResourceStore<byte[]> resourceStore;
	public MaterialStore ( IResourceStore<byte[]> resourceStore ) {
		this.resourceStore = resourceStore;
	}

	Dictionary<(string material, string name), Material> sharedMaterials = new();
	public Material GetShared ( string material, string sharedName ) {
		if ( !sharedMaterials.TryGetValue( (material, sharedName), out var mat ) )
			sharedMaterials.Add( (material, sharedName), mat = GetNew( material ) );

		return mat;
	}

	public Material GetNew ( string material ) {
		return new( GetShader( material ) );
	}

	Dictionary<string, Shader> cachedShaders = new();
	public Shader GetShader ( string name ) {
		if ( !cachedShaders.TryGetValue( name, out var shader ) ) {
			ShaderPart[] parts;
			if ( resourceStore.Get( $"{name}.geom" ) is byte[] geom ) {
				parts = new ShaderPart[3];

				parts[2] = new( fromBytes( geom ), ShaderType.GeometryShader );
			}
			else {
				parts = new ShaderPart[2];
			}

			parts[0] = new( fromBytes( resourceStore.Get( $"{name}.vert" ) ), ShaderType.VertexShader );
			parts[1] = new( fromBytes( resourceStore.Get( $"{name}.frag" ) ), ShaderType.FragmentShader );

			cachedShaders.Add( name, shader = new( parts ) );
		}

		return shader;
	}

	string fromBytes ( byte[] bytes ) {
		using MemoryStream ms = new MemoryStream( bytes );
		using StreamReader sr = new StreamReader( ms );

		return sr.ReadToEnd();
	}
}