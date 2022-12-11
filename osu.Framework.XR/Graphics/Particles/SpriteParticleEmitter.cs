using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Particles;

/// <summary>
/// A <see cref="ParticleEmitter{T, Tmesh}"/> which draws camera-facing sprites
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract partial class SpriteParticleEmitter<T> : ParticleEmitter<T, BasicMesh> where T : struct, IHasMatrix {
	public SpriteParticleEmitter () {
		Mesh = BasicMesh.UnitQuad;
	}

	protected override ParticleEmitterDrawNode CreateDrawNode3D ( int subtreeIndex )
		=> new( this, subtreeIndex );

	protected class SpriteParticleEmitterDrawNode : ParticleEmitterDrawNode {
		public SpriteParticleEmitterDrawNode ( ParticleEmitter<T, BasicMesh> source, int index ) : base( source, index ) { }

		protected override void Draw ( in T particle, IRenderer renderer, object? ctx = null ) {
			var baseMatrix = particle.Matrix * Matrix;
			var position = baseMatrix.ExtractTranslation();
			var translation = Matrix4.CreateTranslation( position );
			var scale = Matrix4.CreateScale( baseMatrix.ExtractScale() );
			var look = Matrix4.CreateFromQuaternion( ( renderer.ProjectionMatrix.ExtractCameraPosition() - position ).LookRotation() );
			Material.Shader.SetUniform( "mMatrix", scale * look * translation );
			Mesh!.Draw();
		}
	}
}