namespace osu.Framework.XR.Graphics.Meshes;

/// <summary>
/// A descriptor for meshes. This includes vertex attributes.
/// A descriptor is required for automatic <see cref="Graphics.Buffers.IAttributeArray"/> linking
/// </summary>
public class MeshDescriptor {
	public const string Position = "Position";
	public const string UV = "UV";
	public const string Normal = "Normal";
	public const string Color = "Color";

	Dictionary<string, List<(int buffer, int atrib)>> attributesByType = new();
	public IReadOnlyDictionary<string, List<(int buffer, int atrib)>> AttributesByType 
		=> attributesByType;
	public MeshDescriptor SetAttribute ( int bufferIndex, int attributeIndex, string type ) {
		if ( !attributesByType.TryGetValue( type, out var list ) )
			attributesByType.Add( type, list = new() );

		list.Add(( bufferIndex, attributeIndex ));
		return this;
	}
}
