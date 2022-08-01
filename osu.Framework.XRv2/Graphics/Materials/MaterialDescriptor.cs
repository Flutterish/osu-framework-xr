using osu.Framework.XR.Graphics.Meshes;
using System.Reflection;

namespace osu.Framework.XR.Graphics.Materials;

/// <summary>
/// A descriptor for materials. This includes vertex attributes and uniforms.
/// A descriptor is required for automatic <see cref="Graphics.Buffers.IAttributeArray"/> linking
/// </summary>
public class MaterialDescriptor {
	public MaterialDescriptor () { }
	public MaterialDescriptor ( MaterialDescriptor parent ) {
		foreach ( var i in parent.attribsByType ) {
			foreach ( var n in i.Value ) {
				SetAttribute( n, i.Key );
			}
		}
		foreach ( var i in parent.uniforms ) {
			uniforms.Add( i.Key, i.Value.Clone() );
		}
		OnBind = parent.OnBind;
	}

	Dictionary<string, List<string>> attribsByType = new();
	public IReadOnlyList<string>? GetAttributeNames ( string type )
		=> attribsByType.GetValueOrDefault( type );
	public MaterialDescriptor SetAttribute ( string name, string type ) {
		if ( !attribsByType.TryGetValue( type, out var list ) )
			attribsByType.Add( type, list = new() );

		list.Add( name );
		return this;
	}

	Dictionary<string, IDescriptorUniform> uniforms = new();
	public IReadOnlyDictionary<string, IDescriptorUniform> Uniforms => uniforms;
	public MaterialDescriptor SetUniform<T> ( string name, T defaultValue ) {
		uniforms.Add( name, new DescriptorUniform<T>( name, defaultValue ) );
		return this;
	}

	public Action<Material, MaterialStore>? OnBind { get; private set; }
	public MaterialDescriptor SetOnBind ( Action<Material, MaterialStore>? action ) {
		OnBind = action;
		return this;
	}

	public MaterialDescriptor AddOnBind ( Action<Material, MaterialStore> action ) {
		var old = OnBind;
		if ( old != null ) {
			OnBind = (m, s) => { old( m, s ); action( m, s ); };
		}
		else {
			SetOnBind( action );
		}
		return this;
	}

	public readonly Dictionary<MeshDescriptor, (int[] indices, int[] attribs)> LinkCache = new();
}

public interface IDescriptorUniform {
	string Name { get; }
	void ApplyDefault ( IMaterialUniform uniform );

	IDescriptorUniform Clone ();
}

public class DescriptorUniform<T> : IDescriptorUniform {
	public T DefaultValue;
	public string Name { get; }

	public DescriptorUniform ( string name, T defaultValue ) {
		Name = name;
		DefaultValue = defaultValue;
	}

	public void ApplyDefault ( IMaterialUniform uniform ) {
		( (IMaterialUniform<T>)uniform ).Value = DefaultValue;
	}

	public IDescriptorUniform Clone () {
		return new DescriptorUniform<T>( Name, DefaultValue );
	}
}