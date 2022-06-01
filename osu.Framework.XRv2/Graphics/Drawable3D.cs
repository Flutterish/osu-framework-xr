using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Framework.XR.Graphics;

// ideally we would be : Transformable, IDrawable, IDisposable but composite drawable provides us with DI and lifetime management.
// perhaps in the future, as Component will be the base class of Drawable, we could inherit from that
public class Drawable3D : CompositeDrawable {
	new public CompositeDrawable3D? Parent { get; [Friend<CompositeDrawable3D>] internal set; }

	/// <summary>
	/// Specifies where in the render pipeline this drawable appears. By default this is
	/// <see cref="InvalidRenderStage.None"/> (-1) and will not be rendered. The valid values
	/// of this property depend on the containing scene/render pipeline
	/// </summary>
	public Enum RenderStage { get; protected set; } = InvalidRenderStage.None;

	protected sealed override DrawNode? CreateDrawNode ()
		=> throw new InvalidOperationException( "Cannot create a 2D draw node for a 3D drawable. This probably means the drawable is located outside of a scene" );

	/// <inheritdoc cref="DrawNode3D"/>
	protected virtual DrawNode3D? CreateDrawNode3D ()
		=> null;
}

public enum InvalidRenderStage { None = -1 }

/// <summary>
/// A node used for synchronisation of data between the update and draw thread.
/// This is executed with a triple buffer, where one node subtree is read from and another written to.
/// The node chosen for reading is the last written to, and the next node for writing is the most outdated one
/// as long as its not the one currently being read from
/// </summary>
/// <remarks>
/// The data shared this way should be small, like uniforms, textures and matrices as there is essentially 3 duplicates of it.
/// Big data, such as meshes or GPU buffers should be updated with a scheduled <see cref="Allocation.IUpload"/>
/// </remarks>
public abstract class DrawNode3D {
	protected Drawable3D Source { get; }
	public long InvalidationID { get; private set; }

	public DrawNode3D ( Drawable3D source ) {
		Source = source;
	}

	/// <inheritdoc cref="UpdateState"/>
	public void UpdateNode () {
		InvalidationID = Source.InvalidationID;
		UpdateState();
	}

	/// <summary>
	/// (On the update thread) Updates the state of the draw node, usually uniforms such as the local matrix,
	/// or changes applied to its material.
	/// </summary>
	/// <remarks>
	/// Changes such as modifications of a mesh should be updated with a scheduled <see cref="Allocation.IUpload"/>
	/// </remarks>
	protected abstract void UpdateState ();
	public abstract void Draw ();
}