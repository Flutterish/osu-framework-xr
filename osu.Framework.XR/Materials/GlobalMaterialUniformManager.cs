using osu.Framework.XR.Components;
using osu.Framework.XR.Materials.Builtin;
using osuTK;
using System.Collections.Generic;

namespace osu.Framework.XR.Materials {
	public static class GlobalMaterialUniformManager {
		public delegate void MaterialBindHandler ( IMaterial material, Drawable3D.DrawNode3D.DrawSettings settings );

		private static Dictionary<string, MaterialBindHandler> bindHandelrs = new();

		internal static void Bind ( IMaterial material, Drawable3D.DrawNode3D.DrawSettings settings ) {
			if ( bindHandelrs.TryGetValue( material.ResourceName, out var handler ) ) {
				handler( material, settings );
			}
		}

		public static void RegisterBindHandler ( string materialName, MaterialBindHandler handler ) {
			bindHandelrs[ materialName ] = handler;
		}

		static GlobalMaterialUniformManager () {
			RegisterBindHandler( UnlitMaterialDescriptor.Name, (material, settings) => {
				material.GetUniform<Matrix4>( UnlitMaterialDescriptor.WorldToCameraMatrix ).UpdateValue( settings.WorldToCamera );
				material.GetUniform<Matrix4>( UnlitMaterialDescriptor.CameraToClipMatrix ).UpdateValue( settings.CameraToClip );
			} );
		}
	}
}
