namespace osu.Framework.XR.Graphics;

public class Shader {
	ShaderPart[] parts;
	public Shader ( ShaderPart[] parts ) {
		this.parts = parts;
	}

	public GlHandle Handle { get; private set; }
	public void Bind () {
		if ( Handle == 0 ) {
			Handle = GL.CreateProgram();

			foreach ( var i in parts ) {
				i.Compile();
				GL.AttachShader( Handle, i.Handle );
			}

			GL.LinkProgram( Handle );
		}

		GL.UseProgram( Handle );
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