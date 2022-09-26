namespace osu.Framework.XR.Graphics.Materials;

public sealed class MaterialUploader {
	public readonly Material Material;
	Dictionary<string, MaterialValue> values = new();
	List<MaterialValue> valuesArray = new();

	public MaterialUploader ( Material material ) {
		Material = material;
	}

	/// <summary>
	/// Sets a material property
	/// </summary>
	public void Set<T> ( string name, T value ) {
		if ( values.TryGetValue( name, out var val ) ) {
			((MaterialValue<T>)val).Value = value;
			val.Id++;
		}
		else {
			val = new MaterialValue<T>( name, value );
			values.Add( name, val );
			valuesArray.Add( val );
		}
	}

	/// <summary>
	/// Sets a material property if it has not been previously modified
	/// </summary>
	public void SetIfDefault<T> ( string name, T value ) {
		if ( IsDefault( name ) )
			Set( name, value );
	}

	/// <summary>
	/// Whether this property has not been modified
	/// </summary>
	public bool IsDefault ( string name ) {
		return !values.ContainsKey( name );
	}

	/// <summary>
	/// Gets a material property
	/// </summary>
	public T Get<T> ( string name ) {
		return values.TryGetValue( name, out var mv ) && mv is MaterialValue<T> val ? val.Value 
			: Material.Descriptor is MaterialDescriptor d ? d.GetUniformDefault<T>( name )
			: default!;
	}

	int lastUpdateIndex = -1;
	public void UpdateState ( int index ) {
		if ( lastUpdateIndex == index )
			return;

		lastUpdateIndex = index;
		for ( int i = 0; i < valuesArray.Count; i++ ) {
			valuesArray[i].UpdateState( index );
		}
	}

	int lastUploadIndex = -1;
	public void UploadState ( int index ) {
		if ( lastUploadIndex == index )
			return;

		lastUploadIndex = index;
		for ( int i = 0; i < valuesArray.Count; i++ ) {
			valuesArray[i].UploadState( Material, index );
		}
	}

	abstract class MaterialValue {
		public ulong Id = 1;
		public abstract void UpdateState ( int index );
		public abstract void UploadState ( Material mat, int index );
	}

	sealed class MaterialValue<T> : MaterialValue {
		public T Value;
		public string Name;

		ulong[] ids;
		T[] uploaded;

		public MaterialValue ( string name, T value ) {
			Name = name;
			Value = value;
			ids = new ulong[3];
			uploaded = new T[3] { value, value, value };
		}

		public override void UpdateState ( int index ) {
			if ( ids[index] != Id ) {
				ids[index] = Id;
				uploaded[index] = Value;
			}
		}

		public override void UploadState ( Material mat, int index ) {
			mat.TrySetUniform( Name, uploaded[index] );
		}
	}
}