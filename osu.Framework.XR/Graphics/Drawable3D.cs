using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Shaders;
using osuTK.Graphics;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

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
	public event Action<Drawable3D, bool>? VisibilityChanged;

	bool isVisible = true;
	/// <summary>
	/// Whether this drawable should be rendered. 
	/// This does not affect whether it is updated
	/// </summary>
	public virtual bool IsVisible {
		get => isVisible;
		set {
			var prev = IsRendered;
			isVisible = value;
			if ( prev != IsRendered )
				VisibilityChanged?.Invoke( this, IsRendered );
		}
	}

	bool isSupertreeVisible = true;
	/// <summary>
	/// Whether the parent and all its parents are visible
	/// </summary>
	public virtual bool IsSupertreeVisible {
		get => isSupertreeVisible;
		[Friend( typeof( CompositeDrawable3D ) )]
		internal set {
			var prev = IsRendered;
			isSupertreeVisible = value;
			if ( prev != IsRendered )
				VisibilityChanged?.Invoke( this, IsRendered );
		}
	}

	/// <summary>
	/// Whether this drawable is being rendered. 
	/// This is only true when this <see cref="IsVisible"/> and <see cref="IsSupertreeVisible"/> are both <see langword="true"/>
	/// </summary>
	public bool IsRendered => isVisible && IsSupertreeVisible;

	/// <summary>
	/// Opacity value, meaning might depend on the implementer, and does not necessarily propagate down
	/// </summary>
	new public virtual float Alpha {
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		get => base.Alpha;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		set => base.Alpha = value;
	}

	/// <summary>
	/// Colour value, meaning might depend on the implementer, and does not necessarily propagate down.
	/// It is <see cref="ColourInfo"/> for compatibility with transforms, you should use the <see cref="Color4"/> <see cref="Tint"/> instead
	/// </summary>
	new public virtual ColourInfo Colour {
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		get => base.Colour;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		set => base.Colour = value;
	}

	/// <summary>
	/// Colour value, meaning might depend on the implementer, and does not necessarily propagate down
	/// </summary>
	public virtual Color4 Tint {
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		get => Colour.TopLeft;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		set => Colour = value;
	}

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

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			DisposeScheduler.Enqueue( this, d => {
				d.subtreeNodes[0]?.Dispose();
				d.subtreeNodes[1]?.Dispose();
				d.subtreeNodes[2]?.Dispose();
			} );
		}

		base.Dispose( isDisposing );
	}
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
public abstract class DrawNode3D : IDisposable {
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
	public abstract void Draw ( IRenderer renderer, object? ctx = null );

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

	/// <summary>
	/// Links the currently bound <see cref="IAttributeArray"/> with the mesh and material data
	/// </summary>
	public static void LinkAttributeArray ( Shader shader, Mesh mesh, MeshDescriptor meshDescriptor, MaterialDescriptor materialDescriptor ) {
		if ( !materialDescriptor.LinkCache.TryGetValue( meshDescriptor, out var values ) ) {
			List<int> attribLengths = new();
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
			materialDescriptor.LinkCache[meshDescriptor] = values;
		}

		var indices = values.indices;
		var attributes = values.attribs.AsSpan();
		mesh.ElementBuffer?.Bind();
		for ( int i = 0; i < indices.Length; i++ ) {
			var index = indices[i];
			mesh.VertexBuffers[i].Link( shader, attributes.Slice( index, (indices.Length > i + 1 ? indices[i + 1] : attributes.Length) - index ) );
		}
	}

	private static Dictionary<Type, IShader> dummyShaders = new();
	private static IShader getDummyShader ( IRenderer renderer ) {
		if ( !dummyShaders.TryGetValue( renderer.GetType(), out var dummy ) ) {
			var assembly = typeof( IRenderer ).Assembly;
			var shader = assembly.GetType( "osu.Framework.Graphics.OpenGL.Shaders.GLShader" )!;
			var shaderpart = assembly.GetType( "osu.Framework.Graphics.OpenGL.Shaders.GLShaderPart" )!;
			var list = shaderpart.MakeArrayType();
			var empty = typeof( Array ).GetMethod( nameof( Array.Empty ), BindingFlags.Static | BindingFlags.Public )!.MakeGenericMethod( shaderpart );
			var constructor = shader.GetConstructor( BindingFlags.NonPublic | BindingFlags.Instance, new Type[] { renderer.GetType(), typeof( string ), list } )!;
			dummy = (IShader)constructor.Invoke( new object[] { renderer, "", empty.Invoke( null, new object[] { } )! } );

			dummyShaders.Add( renderer.GetType(), dummy );
		}

		return dummy;
	}
	private static MethodInfo? _bindBuffer;
	/// <summary>
	/// Ensures the 2D draw state is valid, and resets the 3D state
	/// </summary>
	public static void SwitchTo2DContext ( IRenderer renderer ) {
		Shader.Unbind();
		Material.Unbind();
		GL.BindVertexArray( 0 );
		_bindBuffer ??= renderer.GetType().GetMethod( "BindBuffer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )!;
		_bindBuffer.Invoke( renderer, new object[] { osuTK.Graphics.ES30.BufferTarget.ElementArrayBuffer, 0 } );
		_bindBuffer.Invoke( renderer, new object[] { osuTK.Graphics.ES30.BufferTarget.ArrayBuffer, 0 } );
		(renderer as Renderer)!.BindShader( getDummyShader( renderer ) );
		(renderer as Renderer)!.UnbindShader( getDummyShader( renderer ) );
	}

	public virtual void Dispose () { }
}