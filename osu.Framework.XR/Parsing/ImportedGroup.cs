namespace osu.Framework.XR.Parsing;

/// <summary>
/// An importred collection of objects
/// </summary>
public class ImportedGroup {
	public readonly List<ImportedObject> Children = new();
	public readonly List<ImportedGroup> ChildGroups = new();
}
