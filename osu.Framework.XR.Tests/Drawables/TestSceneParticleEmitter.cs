using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Particles;
using osuTK;
using System;

namespace osu.Framework.XR.Tests.Drawables;

public partial class TestSceneParticleEmitter : BasicTestScene {
	Emitter emitter;
	public TestSceneParticleEmitter () {
		Scene.Add( emitter = new Emitter() );
		AddSliderStep( "Frequency", 1f, 500f, emitter.Frequency, v => emitter.Frequency = v );
	}

	struct Particle : IHasMatrix {
		public Vector3 Position;
		public Vector3 Velocity;
		public float Time;
		public float Lifetime;

		float scale => MathF.Sin(Time / Lifetime * MathF.PI);

		public Matrix4 Matrix => Matrix4.CreateScale( scale * 0.1f ) * Matrix4.CreateTranslation( Position + Time * Velocity );
	}

	partial class Emitter : SpriteParticleEmitter<Particle> {
		public double Frequency = 1000 / 40f;
		public double Interval => 1000 / Frequency;
		double time;
		protected override void Update () {
			base.Update();
			time += Time.Elapsed;
			while ( ActiveParticles < 30000 && time >= Interval ) {
				time -= Interval;
				Emit();
			}

			if ( ActiveParticles >= 30000 )
				time = 0;
		}

		protected override Particle CreateParticle () => new() {
			Position = new Vector3( RNG.NextSingle(-1, 1), RNG.NextSingle(-1, 1), RNG.NextSingle(-1, 1) ).Normalized() * 10,
			Velocity = new Vector3( RNG.NextSingle(-1, 1), RNG.NextSingle(-1, 1), RNG.NextSingle(-1, 1) ).Normalized() / 1000 * 4,
			Lifetime = RNG.NextSingle( 2000, 3000 )
		};

		protected override bool UpdateParticle ( ref Particle particle, float deltaTime ) {
			particle.Time += deltaTime;
			return particle.Lifetime > particle.Time;
		}

		protected override Material CreateDefaultMaterial ( MaterialStore materials )
			=> materials.GetNew( MaterialNames.Unlit );
	}
}
