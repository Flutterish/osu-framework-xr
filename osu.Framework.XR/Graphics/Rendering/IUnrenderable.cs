namespace osu.Framework.XR.Graphics.Rendering;

/// <summary>
/// Marker interface indicating that a <see cref="Drawable3D"/> should not be rendered, or flattened
/// in the render pipeline. This is useful when you want to use it as draw data for other drawables,
/// but still retain the independence of an individual <see cref="Drawable3D"/>
/// </summary>
public interface IUnrenderable { }
