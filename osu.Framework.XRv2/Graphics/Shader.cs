namespace osu.Framework.XR.Graphics;

public class Shader {
	ShaderPart[] parts;
	public Shader ( ShaderPart[] parts ) {
		this.parts = parts;
	}

	public GlHandle Handle { get; private set; }
	public bool IsCompiled => Handle != 0;

	public void Bind () {
		if ( !IsCompiled )
			compile();

		GL.UseProgram( Handle );
	}

	Dictionary<string, int> attribLocations = new();
	public int GetAttrib ( string name ) {
		if ( !attribLocations.TryGetValue( name, out var location ) ) {
			if ( !IsCompiled )
				compile();

			attribLocations.Add( name, location = GL.GetAttribLocation( Handle, name ) );
		}

		return location;
	}

	Dictionary<string, int> uniformLocations = new();
	int getUniform ( string name ) {
		if ( !uniformLocations.TryGetValue( name, out var location ) )
			uniformLocations.Add( name, location = GL.GetUniformLocation( Handle, name ) );

		return location;
	}

	public void SetUniform ( string name, ref Matrix4 value ) {
		GL.UniformMatrix4( getUniform( name ), true, ref value );
	}

	void compile () {
		Handle = GL.CreateProgram();

		foreach ( var i in parts ) {
			i.Compile();
			GL.AttachShader( Handle, i.Handle );
		}

		GL.LinkProgram( Handle );
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