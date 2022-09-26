using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics.Shaders;
using System.Diagnostics.CodeAnalysis;
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
	public readonly MaterialUploader Uploader;

	public Material ( Shader shader, MaterialDescriptor? descriptor = null, MaterialStore? store = null ) {
		Uploader = new( this );
		Shader = shader;
		SourceStore = store;
		Descriptor = descriptor;

		IUpload upload = new DelegateUpload<Material>( this, static m => m.createUniforms() );
		upload.Enqueue();
	}

	static Regex nameIsNotMaterialUniformRegex = new( "^(m|g)[A-Z_]", RegexOptions.Compiled );

	public bool IsLoaded { get; private set; }

	/// <returns>Whether default material uniforms should *not* be created</returns>
	protected virtual bool PerformCustomLoad ( Dictionary<string, IMaterialUniform> uniforms )
		=> false;

	Dictionary<string, IMaterialUniform> uniforms = null!;
	IMaterialUniform[] uniformArray = null!;
	void createUniforms () {
		uniforms = new Dictionary<string, IMaterialUniform>();
		if ( !PerformCustomLoad( uniforms ) ) {
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

	public bool HasUniform<T> ( string name )
		=> uniforms.TryGetValue( name, out var uniform ) ? uniform is IMaterialUniform<T> : false;

	public bool TryGetUniform<T> ( string name, [NotNullWhen(true)] out IMaterialUniform<T>? uniform ) {
		if ( uniforms.TryGetValue( name, out var u ) && u is IMaterialUniform<T> mat ) {
			uniform = mat;
			return true;
		}
		uniform = null;
		return false;
	}

	/// <summary>
	/// Retreives a material uniform. If the material is bound and a value is updated through
	/// this uniform, it will not be immediately updated - you need to call <see cref="IMaterialUniform.Apply"/>
	/// </summary>
	public IMaterialUniform<T> GetUniform<T> ( string name )
		=> (IMaterialUniform<T>)uniforms[name];

	public bool TrySetUniform<T> ( string name, T value ) {
		if ( TryGetUniform<T>( name, out var mat ) ) {
			mat.Value = value;

			if ( boundMaterial == this )
				mat.Apply();

			return true;
		}
		return false;
	}

	public void SetUniform<T> ( string name, T value ) {
		var mat = GetUniform<T>( name );
		mat.Value = value;

		if ( boundMaterial == this )
			mat.Apply();
	}

	public void SetTexture ( Texture value, string name, string subImage = "subImage" ) {
		var mat = GetUniform<Texture>( name );
		mat.Value = value;
		var mat2 = GetUniform<RectangleF>( subImage );
		mat2.Value = value.GetTextureRect();

		if ( boundMaterial == this )
			mat.Apply();
	}

	public bool TryGet<T> ( string name, out T value ) {
		if ( TryGetUniform<T>( name, out var uniform ) ) {
			value = uniform.Value;
			return true;
		}
		value = default!;
		return false;
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

		if ( SourceStore != null )
			Descriptor?.OnBind?.Invoke( this, SourceStore );
	}

	public static void Unbind () {
		boundMaterial = null;
	}

	public IUpload CreateUpload ( Action<Material> action )
		=> new DelegateUpload<Material>( this, action );
}