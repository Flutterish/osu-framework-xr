﻿using osu.Framework.XR.Allocation;
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.XR.Graphics.Shaders;

/// <summary>
/// A GPU program responsible for drawing
/// </summary>
/// <remarks>
/// A shader is always safe to use on the draw thread, and only safe to *introspect* on other threads when <see cref="IsCompiled"/>
/// </remarks>
public class Shader {
	ShaderPart[] parts;
	public Shader ( ShaderPart[] parts ) {
		this.parts = parts;
		IUpload upload = new DelegateUpload<Shader>( this, static s => s.compile() );
		upload.Enqueue();
	}

	public const string StandardGlobalProjectionName = "gProj";
	public const string StandardLocalMatrixName = "mMatrix";
	public const string StandardNormalMatrixName = "mNormal";

	public GlHandle Handle { get; private set; }
	public bool IsCompiled { get; private set; }

	static Shader? boundShader;
	public void Bind () {
		if ( boundShader == this )
			return;

		boundShader = this;
		GL.UseProgram( Handle );
	}

	public static void Unbind () {
		boundShader = null;
	}

	Dictionary<string, int> attribLocations = new();
	public int GetAttrib ( string name ) {
		if ( !attribLocations.TryGetValue( name, out var location ) ) {
			attribLocations.Add( name, location = GL.GetAttribLocation( Handle, name ) );
		}

		return location;
	}

	Dictionary<string, IUniform> uniforms = new();
	public IEnumerable<KeyValuePair<string, IUniform>> AllUniforms
		=> uniforms;
	public IUniform<T> GetUniform<T> ( string name )
		=> (IUniform<T>)uniforms[name];

	public bool TryGetUniform<T> ( string name, [NotNullWhen(true)] out IUniform<T>? uniform ) {
		if ( uniforms.TryGetValue( name, out var u ) && u is IUniform<T> v ) {
			uniform = v;
			return true;
		}

		uniform = null;
		return false;
	}

	public void SetUniform<T> ( string name, ref T value )
		=> GetUniform<T>( name ).UpdateValue( ref value );

	public void SetUniform<T> ( string name, T value )
		=> GetUniform<T>( name ).Value = value;

	public T GetUniformValue<T> ( string name )
		=> GetUniform<T>( name ).Value;

	/// <returns>Whether default uniforms should *not* be created</returns>
	protected virtual bool PerformCustomCompilation ( Dictionary<string, IUniform> uniforms, ref TextureUnit currentTextureUnit )
		=> false;

	void compile () {
		Handle = GL.CreateProgram();

		foreach ( var i in parts ) {
			i.Compile();
			GL.AttachShader( Handle, i.Handle );
		}

		GL.LinkProgram( Handle );

		TextureUnit unit = TextureUnit.Texture0;
		if ( !PerformCustomCompilation( uniforms, ref unit ) ) {
			GL.GetProgram( Handle, GetProgramParameterName.ActiveUniforms, out int uniformCount );
			for ( int i = 0; i < uniformCount; i++ ) {
				GL.GetActiveUniform( Handle, i, 100, out _, out _, out ActiveUniformType type, out string uniformName );
				var location = GL.GetUniformLocation( Handle, uniformName );

				var uniform = type switch {
					ActiveUniformType.Bool => new BoolUniform { Location = location },
					ActiveUniformType.Int => new IntUniform { Location = location },
					ActiveUniformType.Float => new FloatUniform { Location = location },
					ActiveUniformType.FloatVec2 => new Vector2Uniform { Location = location },
					ActiveUniformType.FloatVec3 => new Vector3Uniform { Location = location },
					ActiveUniformType.FloatVec4 => new Vector4Uniform { Location = location },
					ActiveUniformType.FloatMat3 => new Matrix3Uniform { Location = location },
					ActiveUniformType.FloatMat4 => new Matrix4Uniform { Location = location },
					ActiveUniformType.Sampler2D => new Sampler2DUniform { Location = location, TextureUnit = unit++ },
					_ => (IUniform?)null
				};

				if ( uniform != null )
					uniforms.Add( uniformName, uniform );
			}
		}

		IsCompiled = true;
	}
}

public class ShaderPart {
	public readonly string Source;
	public readonly ShaderType Type;

	public ShaderPart ( string source, ShaderType type ) {
		Source = source;
		Type = type;
	}

	public GlHandle Handle { get; private set; }
	public void Compile () {
		if ( Handle == 0 ) {
			Handle = GL.CreateShader( Type );
			GL.ShaderSource( Handle, Source );
		}
	}
}