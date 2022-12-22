namespace osu.Framework.XR.Parsing;

/// <summary>
/// An importred collection of objects
/// </summary>
public class ImportedCollection {
	public readonly List<ImportedObject> Children = new();
	public readonly List<ImportedCollection> ChildCollections = new();
}
