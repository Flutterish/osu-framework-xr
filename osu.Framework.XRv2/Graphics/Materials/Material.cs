using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics.Shaders;
using System.Text.RegularExpressions;

namespace osu.Framework.XR.Graphics.Materials;

/// <summary>
/// A collection of uniform values
/// </summary>
/// <remarks>
/// Uniforms prefixed with "g" for "global" (ex. "gProj" - values set once per batch) 
/// as well as "m" for "my" (ex. "mMatrix" - local values that change every frame) arent considered material uniforms
/// (exception being if the next letter is also lowercase - use an underscore if this is intentional).
/// 
/// A material is always safe to use on the draw thread, and only safe to *introspect* on other threads when <see cref="IsLoaded"/>.
/// You can update values from other threads with <see cref="CreateUpload(Action{Material})"/>
/// </remarks>
public class Material {
	public readonly Shader Shader;
	public readonly MaterialStore? SourceStore;
	public readonly MaterialDescriptor? Descriptor;

	public Material ( Shader shader, MaterialDescriptor? descriptor = null, MaterialStore? store = null ) {
		Shader = shader;
		SourceStore = store;
		Descriptor = descriptor;

		IUpload upload = new DelegateUpload<Material>( this, static m => m.createUniforms() );
		upload.Enqueue();
	}

	static Regex nameIsNotMaterialUniformRegex = new( "^(m|g)[A-Z_]", RegexOptions.Compiled );

	public bool IsLoaded { get; private set; }

	/// <returns>Whether default material uniforms should be created</returns>
	protected virtual bool PerformCustomLoad ( Dictionary<string, IMaterialUniform> uniforms )
		=> true;

	Dictionary<string, IMaterialUniform> uniforms = null!;
	IMaterialUniform[] uniformArray = null!;
	void createUniforms () {
		uniforms = new Dictionary<string, IMaterialUniform>();
		if ( PerformCustomLoad( uniforms ) ) {
			foreach ( var (name, uniform) in Shader.AllUniforms ) {
				if ( nameIsNotMaterialUniformRegex.IsMatch( name ) )
					continue;

				var mat = uniform.CreateMaterialUniform();
				if ( mat != null )
					uniforms.Add( name, mat );
			}

			if ( Descriptor != null ) {
				foreach ( var (name, uniform) in Descriptor.Uniforms ) {
					if ( uniforms.TryGetValue( name, out var mat ) )
						uniform.ApplyDefault( mat );
				}
			}
		}

		uniformArray = uniforms.Values.ToArray();
		IsLoaded = true;
	}

	public IEnumerable<KeyValuePair<string, IMaterialUniform>> AllUniforms
		=> uniforms;

	/// <summary>
	/// Retreives a material uniform. If the material is bound and a value is updated through
	/// this uniform, it will not be immediately updated - you need to call <see cref="IMaterialUniform.Apply"/>
	/// </summary>
	public IMaterialUniform<T> GetUniform<T> ( string name )
		=> (IMaterialUniform<T>)uniforms[name];

	public void Set<T> ( string name, T value ) {
		var mat = GetUniform<T>( name );
		mat.Value = value;

		if ( boundMaterial == this )
			mat.Apply();
	}

	public T Get<T> ( string name )
		=> GetUniform<T>( name ).Value;

	static Material? boundMaterial;
	public void Bind () {
		if ( boundMaterial == this )
			return;

		Shader.Bind();
		boundMaterial = this;

		foreach ( var i in uniformArray ) {
			i.Apply();
		}
	}

	public static void Unbind () {
		boundMaterial = null;
	}

	public IUpload CreateUpload ( Action<Material> action )
		=> new DelegateUpload<Material>( this, action );
}