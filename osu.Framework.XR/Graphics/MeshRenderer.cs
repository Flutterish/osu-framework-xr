using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.XR.Graphics;

/// <summary>
/// A 3D drawable with a mesh and a material
/// </summary>
/// <typeparam name="T">The type of mesh</typeparam>
public abstract partial class MeshRenderer<T> : Drawable3D where T : Mesh {
	protected readonly SharedMeshData<T> SharedData;
	protected virtual SharedMeshData<T> CreateSharedData () => new();

	public MeshRenderer () {
		SharedData = CreateSharedData();
	}

	T? mesh;
	bool ownMesh = false;
	[AllowNull]
	public T Mesh {
		get {
			if ( mesh is null ) {
				mesh = CreateOwnMesh();
				ownMesh = true;
			}
			return mesh;
		}
		set {
			if ( ownMesh )
				mesh!.Dispose();

			if ( mesh?.Descriptor != value?.Descriptor )
				SharedData.DescriptorId++;
			mesh = value;
			ownMesh = false;
			SharedData.MeshId++;
			Invalidate( Invalidation.DrawNode );
		}
	}

	/// <summary>
	/// Creates a default mesh when <see cref="Mesh"/> is accessed but not set
	/// </summary>
	protected virtual T CreateOwnMesh () {
		throw new InvalidOperationException( $"{GetType().ReadableName()} can not create its own mesh" );
	}

	Material? material;
	public Material Material {
		get => material!;
		set {
			material = value;
			if ( material.Shader != value.Shader )
				SharedData.DescriptorId++;
			Invalidate( Invalidation.DrawNode );
		}
	}

	protected abstract Material CreateDefaultMaterial ( MaterialStore materials );

	[BackgroundDependencyLoader]
	private void load ( MaterialStore materials ) {
		material ??= CreateDefaultMaterial( materials );
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			if ( ownMesh )
				mesh!.Dispose();
			SharedData.Dispose();
		}

		base.Dispose( isDisposing );
	}

	protected abstract override MeshRendererDrawNode? CreateDrawNode3D ( int index );

	protected abstract class MeshRendererDrawNode : DrawNode3D {
		new protected MeshRenderer<T> Source => (MeshRenderer<T>)base.Source;
		protected readonly int NodeIndex;
		public MeshRendererDrawNode ( MeshRenderer<T> source, int index ) : base( source ) {
			NodeIndex = index;
			SharedData = source.SharedData;
		}

		protected readonly SharedMeshData<T> SharedData;
		protected Material Material = null!;
		protected T? Mesh;
		protected Matrix4 Matrix;

		ulong linkId;
		ulong meshId;
		protected override void UpdateState () {
			Mesh = Source.mesh!;
			Material = Source.Material;
			Matrix = Source.Matrix;
			linkId = SharedData.DescriptorId;
			meshId = SharedData.MeshId;

			Material.UpdateProperties( NodeIndex );
		}

		/// <summary>
		/// Binds the VAO and material
		/// </summary>
		protected void Bind () {
			if ( linkId > SharedData.DescriptorId ) {
				SharedData.VAO.Clear();
				SharedData.LinkedDescriptorId = linkId;
			}

			if ( SharedData.VAO.Bind() || meshId > SharedData.LinkedMeshId ) {
				LinkAttributeArray( Mesh!, Material );
				SharedData.LinkedMeshId = meshId;
			}

			Material.Bind( NodeIndex );
		}
	}
}

/// <summary>
/// Data about a mesh shared between the update and draw thread
/// </summary>
/// <typeparam name="T">The type of mesh</typeparam>
public class SharedMeshData<T> : IDisposable where T : Mesh {
	/// <summary>
	/// Invalidation ID for mesh and material descriptors (Update Thread)
	/// </summary>
	public ulong DescriptorId = 1;
	/// <summary>
	/// Invalidation ID for mesh and material descriptors (Draw Thread)
	/// </summary>
	public ulong LinkedDescriptorId = 0;
	/// <summary>
	/// Invalidation ID for mesh (Update Thread)
	/// </summary>
	public ulong MeshId = 0;
	/// <summary>
	/// Invalidation ID for mesh (Draw Thread)
	/// </summary>
	public ulong LinkedMeshId = 0;
	public readonly AttributeArray VAO = new();

	public virtual void Dispose () {
		VAO.Dispose();
	}
}