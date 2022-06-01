using osu.Framework.Graphics.Primitives;
using osu.Framework.XR.Graphics.Shaders;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics.Materials;

public interface IMaterialUniform {
	void Apply ();
}

public interface IMaterialUniform<T> : IMaterialUniform {
	T Value { get; set; }
}

public class MaterialUniform<T> : IMaterialUniform<T> {
	public IUniform<T> Source { get; }
	public MaterialUniform ( IUniform<T> source ) {
		Source = source;
	}

	public T Value { get; set; } = default!;

	public void Apply () {
		Source.Value = Value;
	}
}

public class Vector4MaterialUniform : IMaterialUniform<Vector4>, IMaterialUniform<Color4>, IMaterialUniform<RectangleF> {
	public IUniform<Vector4> Source { get; }
	public Vector4MaterialUniform ( Vector4Uniform source ) {
		Source = source;
	}

	Vector4 value;
	Vector4 IMaterialUniform<Vector4>.Value { get => value; set => this.value = value; }
	Color4 IMaterialUniform<Color4>.Value {
		get => new( value.X, value.Y, value.Z, value.W );
		set => this.value = new( value.R, value.G, value.B, value.A );
	}
	RectangleF IMaterialUniform<RectangleF>.Value {
		get => new( value.X, value.Y, value.Z, value.W );
		set => this.value = new( value.X, value.Y, value.Width, value.Height );
	}

	public void Apply () {
		Source.Value = value;
	}
}