using osu.Framework.Graphics.Rendering;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics.Rendering;
using osuTK;
using osuTK.Graphics;
using System;

namespace osu.Framework.XR.Graphics;

public partial class BatchDrawableSusieCube : Drawable3D, IUnrenderable {
	public BatchDrawableSusieCube () {
		color = new Color4( RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1 );
	}

	Color4 color;
	protected override void Update () {
		Rotation *= Quaternion.FromAxisAngle( new Vector3( MathF.Sin( X + Y ), MathF.Cos( (float)Time.Current / 1000 ), MathF.Sin( Z - Y ) ).Normalized(), (float)Time.Elapsed / 1000 );
	}

	protected override DrawNode3D? CreateDrawNode3D ()
		=> new BatchedSusieCubeDrawNode( this );

	public class BatchedSusieCubeDrawNode : DrawNode3D {
		new protected BatchDrawableSusieCube Source => (BatchDrawableSusieCube)base.Source;

		public BatchedSusieCubeDrawNode ( BatchDrawableSusieCube source ) : base( source ) {
			color = source.color;
		}

		public Color4 color;
		public Matrix4 matrix;
		protected override void UpdateState () {
			matrix = Source.Matrix;
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) { }
	}
}
