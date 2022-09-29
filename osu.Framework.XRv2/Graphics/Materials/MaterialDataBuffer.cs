using osu.Framework.XR.Statistics;

namespace osu.Framework.XR.Graphics.Materials;

// TODO probably merge this into Material
public sealed class MaterialDataBuffer {
	public readonly Material Material;
	Dictionary<string, MaterialValue> values = new();
	List<MaterialValue> valuesArray = new();

	public MaterialDataBuffer ( Material material ) {
		Material = material;
	}

	ulong changeId;
	/// <summary>
	/// Sets a material property
	/// </summary>
	public void Set<T> ( string name, T value ) {
		if ( values.TryGetValue( name, out var val ) ) {
			((MaterialValue<T>)val).Value = value;
		}
		else {
			val = new MaterialValue<T>( name, value );
			values.Add( name, val );
			valuesArray.Add( val );
		}

		changeId++;
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

	ulong[] updateIds = new ulong[] { 0, 0, 0 };
	public void UpdateState ( int index ) {
		// prevents duplicate uploads in the same frame (when no change actually happened)
		if ( updateIds[index] == changeId )
			return;

		updateIds[index] = changeId;
		FrameStatistics.Increment( StatisticsCounterType.MaterialUpdate );
		for ( int i = 0; i < valuesArray.Count; i++ ) {
			valuesArray[i].UpdateState( index );
		}
	}

	ulong[] uploadIds = new ulong[] { 0, 0, 0 };
	public void UploadState ( int index ) {
		// prevents duplicate uploads in the same frame (when no change actually happened)
		if ( uploadIds[index] >= updateIds[index] )
			return;

		uploadIds[index] = updateIds[index];
		for ( int i = 0; i < valuesArray.Count; i++ ) {
			valuesArray[i].UploadState( Material, index );
		}
	}

	abstract class MaterialValue {
		public abstract void UpdateState ( int index );
		public abstract void UploadState ( Material mat, int index );
	}

	sealed class MaterialValue<T> : MaterialValue {
		public T Value;
		public string Name;

		T[] uploaded;

		public MaterialValue ( string name, T value ) {
			Name = name;
			Value = value;
			uploaded = new T[3] { value, value, value };
		}

		public override void UpdateState ( int index ) {
			uploaded[index] = Value;
		}

		public override void UploadState ( Material mat, int index ) {
			mat.TrySetUniform( Name, uploaded[index] );
		}
	}
}