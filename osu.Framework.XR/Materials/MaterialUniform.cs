using osu.Framework.Graphics.Shaders;
using System;

namespace osu.Framework.XR.Materials {
	public class MaterialUniform<T> : IMaterialUniform where T : struct, IEquatable<T> {
		Uniform<T> uniform;
		private T Value;

		internal MaterialUniform ( Uniform<T> uniform ) {
			this.uniform = uniform;
		}

		public void Update () {
			uniform.UpdateValue( ref Value );
		}

		public void UpdateValue ( ref T value ) {
			Value = value;
			uniform.UpdateValue( ref value );
		}
		public void UpdateValue ( T value ) {
			Value = value;
			uniform.UpdateValue( ref value );
		}

		public string Name => uniform.Name;
	}
}
