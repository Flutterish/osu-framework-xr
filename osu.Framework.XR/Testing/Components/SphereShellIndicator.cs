using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osuTK;
using System;

namespace osu.Framework.XR.Testing.Components {
	public class SphereShellIndicator : PointIndicator {
		public readonly Bindable<float> RadiusBindable = new( 0.4f );
		public float Radius {
			get => RadiusBindable.Value;
			set => RadiusBindable.Value = value;
		}

		Model shell;
		public SphereShellIndicator ( Scene scene ) : base( scene ) {
			shell = new Model();

			shell.Mesh.AddCircularArc( Vector3.UnitY, Vector3.UnitZ, MathF.Tau, 0.95f, 1, 32 );
			shell.Mesh.AddCircularArc( Vector3.UnitX, Vector3.UnitZ, MathF.Tau, 0.95f, 1, 32 );
			shell.Mesh.AddCircularArc( Vector3.UnitZ, Vector3.UnitX, MathF.Tau, 0.95f, 1, 32 );

			Current.BindValueChanged( v => {
				shell.GlobalPosition = v.NewValue;
			}, true );

			scene.Add( shell );

			RadiusBindable.BindValueChanged( v => {
				shell.Scale = new Vector3( v.NewValue );
			}, true );
		}

		public Colour4 Tint {
			get => shell.Tint;
			set => shell.Tint = value;
		}
	}
}
