namespace osu.Framework.XR;

/// <summary>
/// Specifies that an <see langword="internal"/> modifier is valid only for a specified type
/// </summary>
[AttributeUsage( AttributeTargets.All, AllowMultiple = true, Inherited = true )]
public class FriendAttribute<T> : Attribute { }
