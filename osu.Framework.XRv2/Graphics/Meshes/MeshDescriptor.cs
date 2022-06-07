namespace osu.Framework.XR.Graphics.Meshes;

/// <summary>
/// A descriptor for meshes. This includes vertex attributes.
/// A descriptor is required for automatic <see cref="Graphics.Buffers.IAttributeArray"/> linking
/// </summary>
public class MeshDescriptor {
	public static readonly string Position = "Position";
	public static readonly string UV = "UV";
	public static readonly string Normal = "Normal";
	public static readonly string Color = "Color";

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
