using osuTK;
using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Framework.XR.Parsing.Materials {
	public class MTLMaterial : IImportedMaterial {
		public MTLMaterial ( string name ) {
			Name = name;
		}

		public readonly string Name;
		public Color4? Ambient;
		public Color4? Diffuse;
		public Color4? Specular;
		public Color4? EmissiveCoefficient;
		public Color4? TransmissionFilter;
		public IlluminationMode IlluminationMode = IlluminationMode.Mode1;
		public float Opacity = 1;
		public bool UseHaloOpacity = false;
		public float? SpecularExponent;
		public float Sharpness = 60;
		public float OpticalDensity = 1;
		public TextureMap? AmbientMap;
		public TextureMap? DiffuseMap;
		public TextureMap? SpecularMap;
		public TextureMap? TransmissionFilterMap;
		public TextureMap? OpacityMap;
		public bool UseAntiAliasing = false;
		public TextureMap? DecalMap;
		public TextureMap? DisplacementMap;
		public TextureMap? BumpMap;
		public Dictionary<ReflectionMapType, TextureMap> ReflectionMaps = new();

		public ImportedMaterial CreateMaterial () {
			return new ImportedMaterial();
		}
	}

	public class TextureMap {
		public string? Path;
		public bool BlendU = true;
		public bool BlendV = true;
		public float BumpMultiplier = 1;
		public float Boost = 0;
		public bool UseColorCorrection = false;
		public bool ClampUV = false;
		public Channel ScalarChannel = Channel.Luminance;
		public float RangeBase = 0;
		public float RangeGain = 1;
		public Vector3 Offset;
		public Vector3 PatternScale = new Vector3( 1 );
		public Vector3 Turbulence;
		public long? TextureResolution;

		public TextureMap ( string? path, string[] options, bool isDecal = false ) {
			Path = path;

			if ( isDecal )
				ScalarChannel = Channel.Matte;

			for ( int i = 0; i < options.Length; i++ ) {
				var option = options[ i ];

				if ( option == "-blendu" ) {
					option = options[ ++i ];
					BlendU = option == "on";
				}
				else if ( option == "-blendv" ) {
					option = options[ ++i ];
					BlendV = option == "on";
				}
				else if ( option == "-bm" ) {
					option = options[ ++i ];
					BumpMultiplier = float.Parse( option );
				}
				else if ( option == "-boost" ) {
					option = options[ ++i ];
					Boost = float.Parse( option );
				}
				else if ( option == "-cc" ) {
					option = options[ ++i ];
					UseColorCorrection = option == "on";
				}
				else if ( option == "-clamp" ) {
					option = options[ ++i ];
					ClampUV = option == "on";
				}
				else if ( option == "-imfchan" ) {
					option = options[ ++i ];
					ScalarChannel = option switch {
						"r" => Channel.Red,
						"g" => Channel.Green,
						"b" => Channel.Blue,
						"m" => Channel.Matte,
						"z" => Channel.Depth,
						"l" or _ => Channel.Luminance
					};
				}
				else if ( option == "-mm" ) {
					option = options[ ++i ];
					RangeBase = float.Parse( option );
					option = options[ ++i ];
					RangeGain = float.Parse( option );
				}
				else if ( option == "-o" ) {
					option = options[ ++i ];
					Offset.X = float.Parse( option );
					if ( options.Length <= i || options[ i + 1 ].StartsWith( '-' ) ) continue;
					option = options[ ++i ];
					Offset.Y = float.Parse( option );
					if ( options.Length <= i || options[ i + 1 ].StartsWith( '-' ) ) continue;
					option = options[ ++i ];
					Offset.Z = float.Parse( option );
				}
				else if ( option == "-s" ) {
					option = options[ ++i ];
					PatternScale.X = float.Parse( option );
					if ( options.Length <= i || options[ i + 1 ].StartsWith( '-' ) ) continue;
					option = options[ ++i ];
					PatternScale.Y = float.Parse( option );
					if ( options.Length <= i || options[ i + 1 ].StartsWith( '-' ) ) continue;
					option = options[ ++i ];
					PatternScale.Z = float.Parse( option );
				}
				else if ( option == "-t" ) {
					option = options[ ++i ];
					Turbulence.X = float.Parse( option );
					if ( options.Length <= i || options[ i + 1 ].StartsWith( '-' ) ) continue;
					option = options[ ++i ];
					Turbulence.Y = float.Parse( option );
					if ( options.Length <= i || options[ i + 1 ].StartsWith( '-' ) ) continue;
					option = options[ ++i ];
					Turbulence.Z = float.Parse( option );
				}
				else if ( option == "-textres" ) {
					option = options[ ++i ];
					TextureResolution = long.Parse( option );
				}
			}
		}
	}

	public enum Channel {
		Red,
		Green,
		Blue,
		Matte,
		Luminance,
		Depth
	}

	public enum ReflectionMapType {
		Sphere,
		CubeTop,
		CubeBottom,
		CubeLeft,
		CubeRight,
		CubeBack,
		CubeFront
	}
}
