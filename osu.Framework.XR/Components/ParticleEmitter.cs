using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Rendering;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Components {
	public abstract class ParticleEmitter<T> : ParticleEmiter where T : ParticleEmiter.Particle {
		protected override abstract T CreateParticle ();
		new protected T Emit () => (T)base.Emit();
	}

	public abstract class ParticleEmiter : CompositeDrawable3D {
		private readonly List<Particle> particlePool = new();

		public int ActiveParticles { get; private set; } = 0;
		protected abstract Particle CreateParticle ();
		private Particle getParticle () {
			var particle = particlePool.FirstOrDefault( x => !x.IsApplied );
			if ( particle is null ) {
				particlePool.Add( particle = CreateParticle() );
			}
			return particle;
		}

		private void release ( Particle particle ) {
			Remove( particle );
			ActiveParticles--;
		}
		protected Particle Emit () {
			var particle = getParticle();

			Add( particle );
			ActiveParticles++;
			particle.Apply( this );

			return particle;
		}

		public class Particle : Model {
			private static Mesh? _quadMesh;
			private static Mesh quadMesh {
				get {
					if ( _quadMesh is null ) {
						_quadMesh = new();
						_quadMesh.AddQuad( new Quad(
							new Vector3( -0.5f, 0.5f, 0 ),
							new Vector3( 0.5f, 0.5f, 0 ),
							new Vector3( -0.5f, -0.5f, 0 ),
							new Vector3( 0.5f, -0.5f, 0 )
						) );
					}
					return _quadMesh;
				}
			}

			ParticleEmiter? emmiter;
			public bool IsApplied { get; private set; }
			public Particle () {
				Mesh = quadMesh;
				Scale = new Vector3( 0.03f );
			}

			public void Apply ( ParticleEmiter emmiter ) {
				if ( IsApplied ) throw new InvalidOperationException( "Cannot apply an already applied praticle" );

				IsApplied = true;
				OnApply( this.emmiter = emmiter );
			}
			protected virtual void OnApply ( ParticleEmiter emmiter ) { }

			public void Release () {
				if ( !IsApplied ) throw new InvalidOperationException( "Cannot release a non-applied praticle" );

				IsApplied = false;
				OnReleased();

				emmiter!.release( this );
			}
			protected virtual void OnReleased () { }

			protected override DrawNode3D CreateDrawNode ()
				=> new CameraRotationTrackingDrawNode( this );
		}
	}
}
