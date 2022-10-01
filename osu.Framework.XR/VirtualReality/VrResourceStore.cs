using OpenVR.NET.Devices;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics.Meshes;
using System.Collections.Concurrent;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.VirtualReality;

/// <summary>
/// A simple container for unmanged <see cref="OpenVR.NET"/> resources
/// which allows to load, register their usage and automatically free them when no longer used
/// </summary>
public class VrResourceStore : IDisposable {
	ConcurrentDictionary<int, Task<Texture?>> textures = new();
	Dictionary<ComponentModel, int> usedModels = new();

	/// <summary>
	/// Loads a component mesh and textrure (with caching). This automatically reserves and frees the unmanged resources
	/// </summary>
	public async Task<(Texture? texture, BasicMesh mesh, ComponentModel.ComponentType type)?> LoadComponent ( ComponentModel source, IRenderer renderer, Func<ComponentModel.ComponentType, bool>? loadPredicate = null ) {
		BasicMesh mesh = null!;
		Task<Texture?>? texture = null;
		ComponentModel.ComponentType type = ComponentModel.ComponentType.Error;
		ReserveResources( source );
		await source.LoadAsync(
			begin: t => {
				if ( loadPredicate is null || loadPredicate( t ) ) {
					mesh = new();
					return true;
				}
				else {
					ReleaseResources( source );
					return false;
				}
			},
			finish: t => {
				type = t;
				ReleaseResources( source );
				mesh.CreateFullUnsafeUpload().Enqueue();
			},
			addVertice: ( pos, norm, uv ) => mesh.VertexBuffer.Data.Add( new() {
				Position = pos.ToOsuTk(),
				Normal = norm.ToOsuTk(),
				UV = uv.ToOsuTk()
			} ),
			addTriangle: ( a, b, c ) => mesh.AddFace( (uint)a, (uint)b, (uint)c ),
			addTexture: tex => {
				texture = GetTexture( source, tex, renderer );
			}
		);

		if ( type is ComponentModel.ComponentType.Error )
			return null;

		Texture? tx = texture is null ? null : await texture;
		return (tx, mesh, type);
	}

	/// <summary>
	/// Loads a texture (with caching). This automatically reserves and frees the unmanged resource
	/// </summary>
	public Task<Texture?> GetTexture ( ComponentModel source, ComponentModel.Texture data, IRenderer renderer ) {
		return textures.GetOrAdd( data.ID, async _ => {
			ReserveResources( source );
			var image = await data.LoadImage( flipVertically: true );

			if ( image == null ) {
				ReleaseResources( source );
				return null;
			}

			var tx = renderer.CreateTexture( image.Width, image.Height );
			tx.SetData( new TextureUpload( image ) );
			ReleaseResources( source );
			return tx;
		} );
	}

	/// <summary>
	/// Reserves an unmanaged resource. It will not be unloaded until all resources are released.
	/// A single resource might be reserved several times to indicate it is used in several places
	/// </summary>
	public void ReserveResources ( ComponentModel source ) {
		lock ( usedModels ) {
			if ( usedModels.ContainsKey( source ) )
				usedModels[source]++;
			else
				usedModels.Add( source, 1 );
		}
	}

	/// <summary>
	/// Releases an unmanged resource. Resources will only be unloaded when all resources are released
	/// </summary>
	public void ReleaseResources ( ComponentModel source ) {
		lock ( usedModels ) {
			usedModels[source]--;
			if ( usedModels.All( x => x.Value == 0 ) ) {
				freeResources();
			}
		}
	}

	void freeResources () {
		lock ( usedModels ) {
			foreach ( var (i, _) in usedModels ) {
				i.FreeResources();
			}

			usedModels.Clear();
		}
	}

	public void Dispose () {
		foreach ( var i in textures.Values ) {
			i.Result?.Dispose();
		}
	}
}
