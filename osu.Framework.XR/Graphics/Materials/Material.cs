using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics.Shaders;
using osu.Framework.XR.Statistics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace osu.Framework.XR.Graphics.Materials;

/// <summary>
/// A collection of uniform values
/// </summary>
/// <remarks>
/// Uniforms prefixed with "g" for "global" (ex. "gProj" - values set once per batch) 
/// as well as "m" for "my" (ex. "mMatrix" - local values that change every frame) aren't considered material uniforms
/// (exception being if the next letter is also lowercase [matrixXyz] - use an underscore if this is intentional [m_matrixXyz]).
/// </remarks>
public class Material {
	public readonly Shader Shader;
	public readonly MaterialStore? SourceStore;
	public readonly MaterialDescriptor? Descriptor;
	readonly MaterialDataBuffer dataBuffer;

	public const string StandardPositionAttributeName = "aPos";
	public const string StandardUvAttributeName = "aUv";
	public const string StandardNormalAttributeName = "aNorm";
	public const string StandardVertexColourAttributeName = "aColour";

	public const string StandardTintName = "tint";
	public const string StandardUseGammaName = "useGamma";
	public const string StandardTextureName = "tex";
	public const string StandardTextureRectName = "subImage";

	public Material ( Shader shader, MaterialDescriptor? descriptor = null, MaterialStore? store = null ) {
		dataBuffer = new( this );
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
		LoadComplete();
		IsLoaded = true;
	}

	protected virtual void LoadComplete () { }

	/// <summary>
	/// Enumerates all uniforms.
	/// This is safe to use for introspection on the update thread when <see cref="IsLoaded"/> is <see langword="true"/>
	/// </summary>
	/// <remarks>
	/// If the material is bound and a value is updated through a uniform, 
	/// it will not be immediately updated - you need to call <see cref="IMaterialUniform.Apply"/>.
	/// </remarks>
	public IEnumerable<KeyValuePair<string, IMaterialUniform>> AllUniforms
		=> uniforms;

	/// <summary>
	/// Checks whether a uniform with a given name exists.
	/// This is safe to use for introspection on the update thread when <see cref="IsLoaded"/> is <see langword="true"/>
	/// </summary>
	public bool HasUniform<T> ( string name )
		=> uniforms.TryGetValue( name, out var uniform ) ? uniform is IMaterialUniform<T> : false;

	/// <summary>
	/// Attempts to retreive a uniform.
	/// This is safe to use for introspection on the update thread when <see cref="IsLoaded"/> is <see langword="true"/>
	/// </summary>
	/// <remarks>
	/// If the material is bound and a value is updated through this uniform, 
	/// it will not be immediately updated - you need to call <see cref="IMaterialUniform.Apply"/>.
	/// </remarks>
	public bool TryGetUniform<T> ( string name, [NotNullWhen(true)] out IMaterialUniform<T>? uniform ) {
		if ( uniforms.TryGetValue( name, out var u ) && u is IMaterialUniform<T> mat ) {
			uniform = mat;
			return true;
		}
		uniform = null;
		return false;
	}

	/// <summary>
	/// Retreives a material uniform.
	/// This is safe to use for introspection on the update thread when <see cref="IsLoaded"/> is <see langword="true"/>
	/// </summary>
	/// <remarks>
	/// If the material is bound and a value is updated through this uniform, 
	/// it will not be immediately updated - you need to call <see cref="IMaterialUniform.Apply"/>.
	/// </remarks>
	public IMaterialUniform<T> GetUniform<T> ( string name )
		=> (IMaterialUniform<T>)uniforms[name];

	/// <summary>
	/// Sets a uniform value if that uniform exists
	/// </summary>
	public bool TrySetUniform<T> ( string name, T value ) {
		if ( TryGetUniform<T>( name, out var mat ) ) {
			mat.Value = value;

			if ( boundMaterial == this )
				mat.Apply();

			return true;
		}
		return false;
	}

	/// <summary>
	/// Sets a uniform value
	/// </summary>
	public void SetUniform<T> ( string name, T value ) {
		var mat = GetUniform<T>( name );
		mat.Value = value;

		if ( boundMaterial == this )
			mat.Apply();
	}

	/// <summary>
	/// Sets a uniform sampler2D and its subimage rect
	/// </summary>
	public void SetTextureUniform ( Texture value, string name = StandardTextureName, string subImage = StandardTextureRectName ) {
		var mat = GetUniform<Texture>( name );
		mat.Value = value;
		var mat2 = GetUniform<RectangleF>( subImage );
		mat2.Value = value.GetTextureRect();

		if ( boundMaterial == this )
			mat.Apply();
	}

	/// <summary>
	/// Retreives a uniform value if that uniform exists
	/// </summary>
	public bool TryGetUniformValue<T> ( string name, out T value ) {
		if ( TryGetUniform<T>( name, out var uniform ) ) {
			value = uniform.Value;
			return true;
		}
		value = default!;
		return false;
	}

	/// <summary>
	/// Retreives a uniform value
	/// </summary>
	public T GetUniformValue<T> ( string name )
		=> GetUniform<T>( name ).Value;

	/// <summary>
	/// Sets a material property
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void Set<T> ( string name, T value ) {
		dataBuffer.Set( name, value );
	}

	/// <summary>
	/// Sets a texture and its subimage rect
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetTexture ( string name, Texture value, string subImageName = StandardTextureRectName ) {
		Set( name, value );
		Set( subImageName, value.GetTextureRect() );
	}

	/// <summary>
	/// Sets a material property if it has not been previously modified
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetIfDefault<T> ( string name, T value ) {
		dataBuffer.SetIfDefault( name, value );
	}

	/// <summary>
	/// Whether this property has not been modified
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public bool IsDefault ( string name ) {
		return dataBuffer.IsDefault( name );
	}

	/// <summary>
	/// Gets a material property
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public T Get<T> ( string name ) {
		return dataBuffer.Get<T>( name );
	}

	/// <summary>
	/// Sends properites to the draw thread at a given node index.
	/// This should be called at <see cref="DrawNode3D.UpdateState"/>
	/// </summary>
	/// <param name="index">The triple buffer index</param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void UpdateProperties ( int index ) {
		dataBuffer.UpdateState( index );
	}

	static Material? boundMaterial;
	/// <summary>
	/// Updates uniforms to the material properties in the data buffer.
	/// Sets uniforms to the values defined by this material.
	/// </summary>
	/// <param name="index">The triple buffer index</param>
	public void Bind ( int index ) { // TODO perhaps it would be possible to move the index from draw nodes to the scene
		if ( boundMaterial == this )
			return;

		Shader.Bind();
		FrameStatistics.Increment( StatisticsCounterType.MaterialBind );
		boundMaterial = this;
		dataBuffer.UploadState( index );

		foreach ( var i in uniformArray ) {
			i.Apply();
		}

		if ( SourceStore != null )
			Descriptor?.OnBind?.Invoke( this, SourceStore );
	}

	/// <summary>
	/// Sets uniforms to the values defined by this material
	/// </summary>
	/// <remarks>
	/// This method ignores the data buffer, and should only be used if you manage this material exclusively on the draw thread
	/// </remarks>
	public void BindUniforms () {
		if ( boundMaterial == this )
			return;

		Shader.Bind();
		FrameStatistics.Increment( StatisticsCounterType.MaterialBind );
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

	/// <summary>
	/// Creates an upload which can mutate the data of this material on the draw thread
	/// </summary>
	public IUpload CreateUpload ( Action<Material> action )
		=> new DelegateUpload<Material>( this, action );
}