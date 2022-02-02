using osu.Framework.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace osu.Framework.XR.Materials {
	public static class IShaderExtensions {
		private static Dictionary<IShader, Func<IMaterialUniform>[]> shaderUniformCache = new();

		internal static IMaterialUniform[] GetAllUniforms ( this IShader shader ) {
			if ( shader is not Shader )
				throw new InvalidOperationException( $"{nameof(IShader)}s that are not {nameof(Shader)}s can not be analized for uniforms" );

			if ( !shaderUniformCache.TryGetValue( shader, out var info ) ) {
				var field = typeof( Shader ).GetField( "uniformsValues", BindingFlags.NonPublic | BindingFlags.Instance )!;
				var reflected = field.GetValue( shader )!;
				var uniforms = (object[])reflected;

				info = uniforms.Where( u => {
					var type = u.GetType();
					return type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Uniform<> );
				} )
				.Select( u => {
					var type = u.GetType();
					var genericType = type.GetGenericArguments()[ 0 ];
					var MUtype = typeof( MaterialUniform<> ).MakeGenericType( genericType );
					var args = new object[] { u };

					return (Func<IMaterialUniform>)( () => {
						return (IMaterialUniform)Activator.CreateInstance( MUtype, BindingFlags.NonPublic | BindingFlags.Instance, null, args, null, null )!;
					} );
				} ).ToArray();

				shaderUniformCache.Add( shader, info );
			}

			return info.Select( x => x() ).ToArray();
		}
	}
}
