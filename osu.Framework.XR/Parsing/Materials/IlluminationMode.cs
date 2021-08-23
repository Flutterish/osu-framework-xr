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

/*
0 Color on and Ambient off
1 Color on and Ambient on
2 Highlight on
3 Reflection on and Ray trace on
4 Transparency: Glass on
Reflection: Ray trace on
5 Reflection: Fresnel on and Ray trace on
6 Transparency: Refraction on
Reflection: Fresnel off and Ray trace on
7 Transparency: Refraction on
Reflection: Fresnel on and Ray trace on
8 Reflection on and Ray trace off
9 Transparency: Glass on
Reflection: Ray trace off
10 Casts shadows onto invisible surfaces
*/
