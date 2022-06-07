using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Shaders;
using System.ComponentModel;

namespace osu.Framework.XR.Graphics;

// ideally we would be : Transformable, IDrawable, IDisposable but composite drawable provides us with DI and lifetime management.
// perhaps in the future, as Component will be the base class of Drawable, we could inherit from that
public partial class Drawable3D : CompositeDrawable {
	CompositeDrawable3D? parent;
	new public CompositeDrawable3D? Parent {
		get => parent;
		[Friend( typeof( CompositeDrawable3D ) )]
		internal set {
			parent = value;
			matrix.Invalidate();
		}
	}
	public Drawable3D Root => Parent?.Root ?? this;

	Enum renderStage = InvalidRenderStage.None;
	/// <summary>
	/// Specifies where in the render pipeline this drawable appears. By default this is
	/// <see cref="InvalidRenderStage.None"/> (-1) and will not be rendered. The valid values
	/// of this property depend on the containing scene/render pipeline
	/// </summary>
	public Enum RenderStage {
		get => renderStage;
		set {
			if ( Enum.Equals( renderStage, value ) )
				return;

			var from = renderStage;
			renderStage = value;
			RenderStageChanged?.Invoke( this, from, value );
		}
	}
	public delegate void RenderStageChangedHandler ( Drawable3D drawable, Enum from, Enum to );
	public event RenderStageChangedHandler? RenderStageChanged;

	/// <summary>
	/// Render layer expressed as a bitfield (or custom via the given render pipeline).
	/// This defines a mask that allows to select which drawables should be rendered.
	/// The default value is 1
	/// </summary>
	public ulong RenderLayer = 1;

	DrawNode3D?[] subtreeNodes = new DrawNode3D?[3];
	public DrawNode3D? GetDrawNodeAtSubtree ( int subtreeIndex ) {
		return subtreeNodes[subtreeIndex] ??= CreateDrawNode3D( subtreeIndex );
	}

	[EditorBrowsable( EditorBrowsableState.Never )]
	protected sealed override DrawNode? CreateDrawNode ()
		=> throw new InvalidOperationException( "Cannot create a 2D draw node for a 3D drawable. This probably means the drawable is located outside of a scene" );

	/// <inheritdoc cref="DrawNode3D"/>
	protected virtual DrawNode3D? CreateDrawNode3D ( int subtreeIndex )
		=> CreateDrawNode3D();

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
	public Drawable3D Source { get; }
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
	/// <summary>
	/// Draws the drawable, or otherwise interacts with the render pipeline
	/// </summary>
	/// <param name="ctx">An optional context to be passed by the render pipeline</param>
	public abstract void Draw ( object? ctx = null );

	/// <summary>
	/// Links the currently bound <see cref="IAttributeArray"/> with the mesh and material data.
	/// This overload will throw if descriptors are not available
	/// </summary>
	public static void LinkAttributeArray ( Mesh mesh, Material material ) {
		if ( mesh.Descriptor is not MeshDescriptor meshDescriptor )
			throw new InvalidOperationException( "Tried to automatically link a mesh with no descriptor" );
		if ( material.Descriptor is not MaterialDescriptor materialDescriptor )
			throw new InvalidOperationException( "Tried to automatically link a material with no descriptor" );

		LinkAttributeArray( material.Shader, mesh, meshDescriptor, materialDescriptor );
	}

	static List<int> attribLengths = new();
	static Dictionary<(Shader, MeshDescriptor, MaterialDescriptor), (int[] indices, int[] attribs)> linkCache = new();
	/// <summary>
	/// Links the currently bound <see cref="IAttributeArray"/> with the mesh and material data
	/// </summary>
	public static void LinkAttributeArray ( Shader shader, Mesh mesh, MeshDescriptor meshDescriptor, MaterialDescriptor materialDescriptor ) {
		var key = (shader, meshDescriptor, materialDescriptor);
		if ( !linkCache.TryGetValue( key, out var values ) ) {
			attribLengths.Clear();
			int attribCount = 0;
			foreach ( var i in meshDescriptor.AttributesByType.Values ) {
				attribCount += i.Count;
				foreach ( var (buffer, index) in i ) {
					while ( attribLengths.Count <= buffer )
						attribLengths.Add( 0 );

					attribLengths[buffer]++;
				}
			}
			int[] attribIndices = new int[attribLengths.Count];
			int offset = 0;
			int k = 0;
			foreach ( var i in attribLengths ) {
				attribIndices[k++] = offset;
				offset += i;
			}
			int[] attribs = new int[attribCount];
			foreach ( var (type, locations) in meshDescriptor.AttributesByType ) {
				var names = materialDescriptor.GetAttributeNames( type );
				for ( int i = 0; i < locations.Count; i++ ) {
					var (buffer, index) = locations[i];
					if ( names?[i] is string name ) {
						attribs[attribIndices[buffer] + index] = shader.GetAttrib( name );
					}
					else {
						attribs[attribIndices[buffer] + index] = -1;
					}
				}
			}
			values = (attribIndices, attribs);
			linkCache.Add( key, values );
		}

		var indices = values.indices;
		var attributes = values.attribs.AsSpan();
		mesh.ElementBuffer?.Bind();
		for ( int i = 0; i < indices.Length; i++ ) {
			mesh.VertexBuffers[i].Link( shader, attributes.Slice( indices[i], attribLengths[i] ) );
		}
	}

	/// <summary>
	/// Ensures the 2D draw state is valid, and resets the 3D state
	/// </summary>
	public static void SwitchTo2DContext () {
		Shaders.Shader.Unbind();
		Material.Unbind();
		GL.BindVertexArray( 0 );
		GLWrapper.BindBuffer( osuTK.Graphics.ES30.BufferTarget.ElementArrayBuffer, 0 );
		GLWrapper.BindBuffer( osuTK.Graphics.ES30.BufferTarget.ArrayBuffer, 0 );
		GLWrapper.UseProgram( 0 );
		GLWrapper.UseProgram( null );
	}
}