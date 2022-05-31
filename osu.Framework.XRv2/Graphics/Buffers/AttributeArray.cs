namespace osu.Framework.XR.Graphics.Buffers;

/// <summary>
/// A (vertex) attribute array is a "link" between geometry data (vertice buffers + element buffer)
/// and a specific shader program (for example the shader of a material).
/// Generally, an attribute array is what constitutes a model, however a single
/// attribute array might be used to render different meshes whose vertex structure is the same
/// by swapping the element buffer. You can also use a single attribute array while swapping
/// shader uniforms, for example to render the same mesh in multiple places or with different textures,
/// or even different animation progresses
/// </summary>
public interface IAttributeArray {

}

/// <inheritdoc cref="IAttributeArray"/>
public class AttributeArray {

}
