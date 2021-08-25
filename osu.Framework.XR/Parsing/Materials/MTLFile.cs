using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Framework.XR.Parsing.Materials {
	public class MTLFile {
		private MTLFile () { }

		public readonly List<ParsingError> ParsingErrors = new();
		public readonly List<(uint line, string content)> Comments = new();

		public readonly List<MTLMaterial> Materials = new();

		public static MTLFile FromFile ( string path )
			=> FromText( File.ReadAllLines( path ) );

		public static MTLFile FromText ( string text )
			=> FromText( text.Split( '\n' ) );

		// https://www.fileformat.info/format/material/
		public static MTLFile FromText ( IEnumerable<string> lines ) {
			MTLFile file = new();
			var errors = file.ParsingErrors;
			MTLMaterial? _material = null;
			MTLMaterial Material() {
				if ( _material is null ) {
					_material = new MTLMaterial( "Untitled Material" );
					file.Materials.Add( _material );
				}
				return _material;
			}

			static string? takeNext ( ref string data ) {
				string? toParse = null;
				var index = data.IndexOf( ' ' );
				if ( index == -1 ) {
					if ( data.Any() ) {
						toParse = data;
						data = "";
						return toParse;
					}
					else {
						return null;
					}
				}
				else {
					toParse = data.Substring( 0, index );
					data = data.Substring( index ).Trim();
					return toParse;
				}
			}

			static Color4 parseReflectivity ( string rest, uint L, List<ParsingError> errors ) {
				var next = takeNext( ref rest );
				if ( next == "xyz" ) {
					errors.Add( new( $"Ka xyz was declared at L{L}, but is not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
				}
				else if ( next == "spectral" ) {
					errors.Add( new( $"Ka spectral was declared at L{L}, but is not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
				}
				else {
					var r = next;
					var g = takeNext( ref rest );
					var b = takeNext( ref rest );

					if ( r is null ) {
						errors.Add( new( $"Ka rgb at L{L} expected 1 or 3 values, but got 0", ParsingErrorSeverity.Issue ) );
					}
					else if ( g is null ) {
						var R = float.Parse( r );
						return new Color4( R, R, R, 1 );
					}
					else if ( b is null ) {
						errors.Add( new( $"Ka rgb at L{L} expected 1 or 3 values, but got 2", ParsingErrorSeverity.Issue ) );
					}
					else {
						return new Color4(
							float.Parse( r ),
							float.Parse( g ),
							float.Parse( b ),
							1
						);
					}
				}
				return Color4.White;
			}

			uint L = 0;
			foreach ( var line in lines ) {
				L++;
				var commentIndex = line.IndexOf( "#" );
				var rest = ( commentIndex == -1 ? line : line.Substring( 0, commentIndex ) ).Trim();

				var type = takeNext( ref rest );

				if ( type is null or "" ) {
					continue;
				}

				try {
					if ( type == "newmtl" ) {
						_material = new MTLMaterial( rest );
						file.Materials.Add( _material );
					}
					else if ( type == "Ka" ) {
						Material().Ambient = parseReflectivity( rest, L, errors );
					}
					else if ( type == "Kd" ) {
						Material().Diffuse = parseReflectivity( rest, L, errors );
					}
					else if ( type == "Ks" ) {
						Material().Specular = parseReflectivity( rest, L, errors );
					}
					else if ( type == "Tf" ) {
						Material().TransmissionFilter = parseReflectivity( rest, L, errors );
					}
					else if ( type == "illum" ) {
						var num = int.Parse( rest );
						if ( num is < 0 or > 10 ) {
							errors.Add( new( $"Illumination mode was specified at L{L}, but {num} is outside the allowed range 0-10.", ParsingErrorSeverity.Issue ) );
						}
						else {
							Material().IlluminationMode = num switch {
								0 => IlluminationMode.Mode0,
								1 => IlluminationMode.Mode1,
								2 => IlluminationMode.Mode2,
								3 => IlluminationMode.Mode3,
								4 => IlluminationMode.Mode4,
								5 => IlluminationMode.Mode5,
								6 => IlluminationMode.Mode6,
								7 => IlluminationMode.Mode7,
								8 => IlluminationMode.Mode8,
								9 => IlluminationMode.Mode9,
								10 or _ => IlluminationMode.Mode10
							};
						}
					}
					else if ( type == "d" ) {
						var next = takeNext( ref rest );
						if ( next == "-halo" ) {
							Material().UseHaloOpacity = true;
							next = rest;
						}
						Material().Opacity = float.Parse( next! );
					}
					else if ( type == "Ns" ) {
						Material().SpecularExponent = float.Parse( rest );
					}
					else if ( type == "sharpness" ) {
						Material().Sharpness = float.Parse( rest );
					}
					else if ( type == "Ni" ) {
						Material().OpticalDensity = float.Parse( rest );
					}
					else if ( type == "map_Ka" ) {
						var parts = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
						Material().AmbientMap = new( parts.LastOrDefault(), parts.SkipLast( 1 ).ToArray() );
					}
					else if ( type == "map_Kd" ) {
						var parts = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
						Material().DiffuseMap = new( parts.LastOrDefault(), parts.SkipLast( 1 ).ToArray() );
					}
					else if ( type == "map_Ks" ) {
						var parts = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
						Material().SpecularMap = new( parts.LastOrDefault(), parts.SkipLast( 1 ).ToArray() );
					}
					else if ( type == "map_Ns" ) {
						var parts = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
						Material().TransmissionFilterMap = new( parts.LastOrDefault(), parts.SkipLast( 1 ).ToArray() );
					}
					else if ( type == "map_d" ) {
						var parts = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
						Material().OpacityMap = new( parts.LastOrDefault(), parts.SkipLast( 1 ).ToArray() );
					}
					else if ( type == "map_aat" ) {
						if ( rest == "on" )
							Material().UseAntiAliasing = true;
					}
					else if ( type == "decal" ) {
						var parts = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
						Material().DecalMap = new( parts.LastOrDefault(), parts.SkipLast( 1 ).ToArray(), isDecal: true );
					}
					else if ( type == "disp" ) {
						var parts = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
						Material().DisplacementMap = new( parts.LastOrDefault(), parts.SkipLast( 1 ).ToArray() );
					}
					else if ( type == "bump" ) {
						var parts = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
						Material().BumpMap = new( parts.LastOrDefault(), parts.SkipLast( 1 ).ToArray() );
					}
					else if ( type == "refl" ) {
						var next = takeNext( ref rest );
						if ( next != "-type" ) {
							errors.Add( new( $"Reflection map at L{L} ommited '-type'.", ParsingErrorSeverity.Issue ) );
						}
						next = takeNext( ref rest );
						ReflectionMapType? mapType = next switch {
							"sphere" => ReflectionMapType.Sphere,
							"cube_top" => ReflectionMapType.CubeTop,
							"cube_bottom" => ReflectionMapType.CubeBottom,
							"cube_front" => ReflectionMapType.CubeFront,
							"cube_back" => ReflectionMapType.CubeBack,
							"cube_left" => ReflectionMapType.CubeLeft,
							"cube_right" => ReflectionMapType.CubeRight,
							_ => null
						};

						if ( mapType is null ) {
							errors.Add( new( $"Reflection map at L{L} did not specify a valid type. The value was: {next}", ParsingErrorSeverity.Issue ) );
							continue;
						}

						var parts = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
						Material().ReflectionMaps.Add( mapType.Value, new TextureMap( parts.LastOrDefault(), parts.SkipLast( 1 ).ToArray() ) );
					}
					else {
						errors.Add( new( $"{type} was declared at L{L}, but its not a recognized identifier.", ParsingErrorSeverity.Issue ) );
					}
				}
				catch ( Exception e ) {
					errors.Add( new( $"Exception while parsing L{L}: {e.Message}", ParsingErrorSeverity.Error ) );
				}
			}

			return file;
		}
	}
}
