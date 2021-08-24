using System;

namespace osu.Framework.XR.Parsing.Materials {
	[Flags]
	public enum IlluminationMode {
		Color = 1,
		Ambient = 2,
		Highlight = 4,
		Reflection = 8,
		RayTrace = 16,
		TransparencyGlass = 32,
		ReflectionRayTrace = 64,
		ReflectionFresnel = 128,
		TransparencyRefraction = 256,
		CastShadowsOnInvisibleSurfaces = 512,


		Mode0 = 0,
		Mode1 = Color | Ambient,
		Mode2 = Highlight,
		Mode3 = Reflection | RayTrace,
		Mode4 = TransparencyGlass | ReflectionRayTrace,
		Mode5 = ReflectionFresnel | ReflectionRayTrace,
		Mode6 = TransparencyRefraction | ReflectionRayTrace,
		Mode7 = TransparencyRefraction | ReflectionFresnel | ReflectionRayTrace,
		Mode8 = Reflection,
		Mode9 = TransparencyGlass,
		Mode10 = CastShadowsOnInvisibleSurfaces
	}
}