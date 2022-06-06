using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics.Materials;
using osuTK.Graphics;

namespace osu.Framework.XR.Graphics.Shaders;

public interface IUniform {
	int Location { get; }
	IMaterialUniform? CreateMaterialUniform ();
}

public interface IUniform<T> : IUniform {
	T Value { get; set; }
	void UpdateValue ( ref T value );

	IMaterialUniform IUniform.CreateMaterialUniform ()
		=> new MaterialUniform<T>( this );
}

public class BoolUniform : IUniform<bool> {
	public int Location { get; init; }
	bool value;
	public bool Value {
		get => value;
		set => UpdateValue( ref value );
	}

	public void UpdateValue ( ref bool value ) {
		if ( this.value == value )
			return;

		this.value = value;
		GL.Uniform1( Location, value ? 1 : 0 );
	}
}

public class FloatUniform : IUniform<float> {
	public int Location { get; init; }
	float value;
	public float Value {
		get => value;
		set => UpdateValue( ref value );
	}

	public void UpdateValue ( ref float value ) {
		if ( this.value == value )
			return;

		this.value = value;
		GL.Uniform1( Location, value );
	}
}

public class IntUniform : IUniform<int> {
	public int Location { get; init; }
	int value;
	public int Value {
		get => value;
		set => UpdateValue( ref value );
	}

	public void UpdateValue ( ref int value ) {
		if ( this.value == value )
			return;

		this.value = value;
		GL.Uniform1( Location, value );
	}
}

public class Vector2Uniform : IUniform<Vector2> {
	public int Location { get; init; }
	Vector2 value;

	public Vector2 Value {
		get => value;
		set => UpdateValue( ref value );
	}

	public void UpdateValue ( ref Vector2 value ) {
		if ( this.value == value )
			return;

		this.value = value;
		GL.Uniform2( Location, ref value );
	}
}

public class Vector3Uniform : IUniform<Vector3> {
	public int Location { get; init; }
	Vector3 value;

	public Vector3 Value {
		get => value;
		set => UpdateValue( ref value );
	}

	public void UpdateValue ( ref Vector3 value ) {
		if ( this.value == value )
			return;

		this.value = value;
		GL.Uniform3( Location, ref value );
	}
}

public class Vector4Uniform : IUniform<Vector4>, IUniform<Color4>, IUniform<RectangleF>, IUniform<RectangleI> {
	IMaterialUniform IUniform.CreateMaterialUniform ()
		=> new Vector4MaterialUniform( this );

	public int Location { get; init; }
	Vector4 value;

	Vector4 IUniform<Vector4>.Value { 
		get => value; 
		set => UpdateValue( ref value );
	}
	Color4 IUniform<Color4>.Value {
		get => new( value.X, value.Y, value.Z, value.W );
		set => UpdateValue( ref value );
	}
	RectangleF IUniform<RectangleF>.Value {
		get => new( value.X, value.Y, value.Z, value.W );
		set => UpdateValue( ref value );
	}
	RectangleI IUniform<RectangleI>.Value {
		get => new( (int)value.X, (int)value.Y, (int)value.Z, (int)value.W );
		set => UpdateValue( ref value );
	}

	public void UpdateValue ( ref Vector4 value ) {
		if ( this.value == value )
			return;

		this.value = value;
		GL.Uniform4( Location, ref value );
	}
	public void UpdateValue ( ref Color4 value ) {
		var v = new Vector4( value.R, value.G, value.B, value.A );
		UpdateValue( ref v );
	}
	public void UpdateValue ( ref RectangleF value ) {
		var v = new Vector4( value.X, value.Y, value.Width, value.Height );
		UpdateValue( ref v );
	}
	public void UpdateValue ( ref RectangleI value ) {
		var v = new Vector4( value.X, value.Y, value.Width, value.Height );
		UpdateValue( ref v );
	}
}

public class Matrix4Uniform : IUniform<Matrix4> {
	IMaterialUniform IUniform.CreateMaterialUniform ()
		=> new MaterialUniform<Matrix4>( this ) { Value = Matrix4.Identity };

	public int Location { get; init; }
	Matrix4 value;
	public Matrix4 Value {
		get => value;
		set => UpdateValue( ref value );
	}

	public void UpdateValue ( ref Matrix4 value ) {
		if ( this.value == value )
			return;

		this.value = value;
		GL.UniformMatrix4( Location, true, ref value );
	}
}

public class Sampler2DUniform : IUniform<Texture?>, IUniform<TextureGL?> {
	IMaterialUniform IUniform.CreateMaterialUniform ()
		=> new Sampler2DMaterialUniform( this );

	public TextureUnit TextureUnit { get; init; }
	public int Location { get; init; }
	Texture? value;
	TextureGL? glvalue;
	Texture? IUniform<Texture?>.Value {
		get => value;
		set => UpdateValue( ref value );
	}
	TextureGL? IUniform<TextureGL?>.Value {
		get => glvalue;
		set => UpdateValue( ref value );
	}

	public void UpdateValue ( ref Texture? value ) {
		this.value = value;
		this.glvalue = value?.TextureGL;
		if ( value is null ) {
			GL.ActiveTexture( TextureUnit );
			GL.BindTexture( TextureTarget.Texture2D, 0 );
		}
		else
			value.TextureGL.Bind( (osuTK.Graphics.ES30.TextureUnit)TextureUnit );
	}
	public void UpdateValue ( ref TextureGL? value ) {
		this.value = null;
		this.glvalue = value;
		if ( value is null ) {
			GL.ActiveTexture( TextureUnit );
			GL.BindTexture( TextureTarget.Texture2D, 0 );
		}
		else
			value.Bind( (osuTK.Graphics.ES30.TextureUnit)TextureUnit );
	}
}