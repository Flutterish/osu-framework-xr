using osu.Framework.IO.Stores;
using osu.Framework.XR.Graphics.Shaders;

namespace osu.Framework.XR.Graphics.Materials;

public class MaterialStore {
	IResourceStore<byte[]> resourceStore;
	public MaterialStore ( IResourceStore<byte[]> resourceStore ) {
		this.resourceStore = resourceStore;
	}

	Dictionary<string, MaterialDescriptor> descriptors = new();
	public void AddDescriptor ( string name, MaterialDescriptor descriptor ) {
		descriptors[name] = descriptor;
	}
	public MaterialDescriptor GetDescriptor ( string name ) {
		return descriptors[name];
	}

	Dictionary<string, object?> globalProperties = new();
	public void SetGlobalProperty<T> ( string name, T value ) { // TODO check if any-Enum based names are better (perf-wise)
		globalProperties[name] = value;
	}
	public T GetGlobalProperty<T> ( string name )
		=> (T)globalProperties[name]!;

	Dictionary<(string material, string name), Material> sharedMaterials = new();
	public bool SharedMaterialExists ( string material, string sharedName ) => sharedMaterials.ContainsKey( (material, sharedName) );
	public Material GetShared ( string material, string sharedName ) => GetShared<Material, Shader>( material, sharedName );
	public Tmaterial GetShared<Tmaterial, Tshader> ( string material, string sharedName ) where Tmaterial : Material where Tshader : Shader {
		if ( !sharedMaterials.TryGetValue( (material, sharedName), out var mat ) )
			sharedMaterials.Add( (material, sharedName), mat = GetNew<Tmaterial, Tshader>( material ) );

		return (Tmaterial)mat;
	}

	public bool GetShared ( string material, string sharedName, out Material mat ) => GetShared<Material, Shader>( material, sharedName, out mat );
	public bool GetShared<Tmaterial, Tshader> ( string material, string sharedName, out Tmaterial mat ) where Tmaterial : Material where Tshader : Shader {
		if ( !sharedMaterials.TryGetValue( (material, sharedName), out var mat2 ) ) {
			sharedMaterials.Add( (material, sharedName), mat = GetNew<Tmaterial, Tshader>( material ) );
			return false;
		}

		mat = (Tmaterial)mat2;
		return true;
	}

	public Material GetNew ( string material ) => GetNew<Material, Shader>( material );
	public Tmaterial GetNew<Tmaterial, Tshader> ( string material ) where Tmaterial : Material where Tshader : Shader {
		var shader = GetShader<Tshader>( material );
		var descriptor = descriptors.GetValueOrDefault( material );
		return (Tmaterial)Activator.CreateInstance( typeof( Tmaterial ), new object?[] { shader, descriptor, this } )!;
	}

	Dictionary<string, Shader> cachedShaders = new();
	public Shader GetShader ( string name ) => GetShader<Shader>( name );
	public Tshader GetShader<Tshader> ( string name ) where Tshader : Shader {
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

			cachedShaders.Add( name, shader = (Tshader)Activator.CreateInstance( typeof( Tshader ), new object[] { parts } )! );
		}

		return (Tshader)shader;
	}

	string fromBytes ( byte[] bytes ) {
		using MemoryStream ms = new MemoryStream( bytes );
		using StreamReader sr = new StreamReader( ms );

		return sr.ReadToEnd();
	}
}