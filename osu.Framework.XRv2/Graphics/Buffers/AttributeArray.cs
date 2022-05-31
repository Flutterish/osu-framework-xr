﻿namespace osu.Framework.XR.Graphics.Buffers;

/// <summary>
/// A "link" between geometry data (<see cref="IVertexBuffer"/>s + <see cref="IElementBuffer"/>)
/// and a specific shader program (for example the shader of a material).
/// </summary>
/// <remarks>
/// Generally, an attribute array is what constitutes a model, however a single
/// attribute array might be used to render different "partial" meshes whose vertice buffers are the same
/// by swapping the element buffer. You can also use a single attribute array while swapping
/// shader uniforms, for example to render the same mesh in multiple places or with different textures,
/// or even different animation progresses
/// </remarks>
public interface IAttributeArray {
	/// <summary>
	/// Binds the <see cref="IAttributeArray"/>. 
	/// All <see cref="IVertexBuffer"/> link calls will be linked to this attribute array.
	/// The next bound <see cref="IElementBuffer"/> will be linked to this attibute array (and any subsequent will replace it).
	/// When binding this attribute array again, the linked buffers will be automatically bound too
	/// </summary>
	void Bind ();
}

/// <inheritdoc cref="IAttributeArray"/>
public class AttributeArray : IAttributeArray {
	public GlHandle Handle { get; private set; }
	
	public void Bind () {
		if ( Handle == 0 ) {
			Handle = GL.GenVertexArray();
		}

		GL.BindVertexArray( Handle );
	}
}
