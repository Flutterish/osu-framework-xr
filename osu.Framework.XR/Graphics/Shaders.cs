using osu.Framework.Graphics.Shaders;
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.XR.Graphics {
	public static class Shaders {
		public static readonly Vertex3DShaderDescription VERTEX_3D = new();
		public static readonly Fragment3DShaderDescription FRAGMENT_3D = new();

		[MaybeNull, NotNull]
		public static Shader Shader3D;
	}

	public abstract class ShaderDescription {
		public ShaderDescription ( string name ) => Name = name;

		public readonly string Name;

		public static implicit operator string ( ShaderDescription description )
			=> description.Name;
	}

	public class Vertex3DShaderDescription : ShaderDescription {
		public Vertex3DShaderDescription () : base( "3DVertex" ) { }
		public readonly string WorldToCameraMatrix = "worldToCamera";
		public readonly string CameraToClipMatrix = "cameraToClip";
		public readonly string LocalToWorldMatrix = "localToWorld";
	}

	public class Fragment3DShaderDescription : ShaderDescription {
		public Fragment3DShaderDescription () : base( "3DFrag" ) { }
		public readonly string UseGammaCorrection = "useGammaCorrection";
		public readonly string Tint = "tint";
	}
}
