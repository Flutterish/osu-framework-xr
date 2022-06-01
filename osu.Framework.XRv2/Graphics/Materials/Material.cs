using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics.Shaders;
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.XR.Graphics.Materials;

public class Material {
	public readonly Shader Shader;

	public Material ( Shader shader ) {
		Shader = shader;
	}

	public bool IsLoaded => Shader.IsCompiled;

	Dictionary<string, IMaterialUniform>? uniforms;
	IMaterialUniform[]? uniformArray;
	[MemberNotNull(nameof(uniforms), nameof(uniformArray))]
	void ensureUniformsCreated () {
		if ( uniforms is null ) {
			uniforms = new();

			Shader.EnsureCompiled();
			uniformArray = new IMaterialUniform[Shader.AllUniforms.Count()];
			int i = 0;
			foreach ( var (name, uniform) in Shader.AllUniforms ) {
				var mat = uniform.CreateMaterialUniform();
				uniforms.Add( name, mat );
				uniformArray[i++] = mat;
			}
		}
	}

	public IEnumerable<KeyValuePair<string, IMaterialUniform>> AllUniforms {
		get {
			ensureUniformsCreated();
			return uniforms;
		}
	}

	public IMaterialUniform<T> GetUniform<T> ( string name ) {
		ensureUniformsCreated();
		return (IMaterialUniform<T>)uniforms[name];
	}

	public void SetUniform<T> ( string name, T value ) {
		ensureUniformsCreated();
		var mat = GetUniform<T>( name );
		mat.Value = value;

		if ( boundMaterial == this )
			mat.Apply();
	}

	public T GetUniformValue<T> ( string name ) {
		ensureUniformsCreated();
		return GetUniform<T>( name ).Value;
	}

	static Material? boundMaterial;
	public void Bind () {
		if ( boundMaterial == this )
			return;

		Shader.Bind();
		boundMaterial = this;

		ensureUniformsCreated();
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