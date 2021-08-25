using osu.Framework.Graphics;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Parsing.Materials;
using osuTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Framework.XR.Parsing.WaveFront {
	public class OBJFile : IModelFile {
		private OBJFile () { }

		public readonly List<ParsingError> ParsingErrors = new();
		public readonly List<(uint line, string content)> Comments = new();

		public readonly OBJData Data = new();
		public readonly List<OBJObject> Objects = new();
		public readonly List<OBJGroup> Groups = new();
		public readonly List<OBJGroup> SmoothingGroups = new();
		public readonly List<OBJGroup> MergingGroups = new();

		public static OBJFile FromFile ( string path )
			=> FromText( File.ReadAllLines( path ) );

		public static OBJFile FromText ( string text )
			=> FromText( text.Split( '\n' ) );

		// http://www.martinreddy.net/gfx/3d/OBJ.spec
		public static OBJFile FromText ( IEnumerable<string> lines ) {
			OBJFile file = new();
			OBJData source = file.Data;
			List<OBJGroup> activeGroups = new();
			OBJGroup? smoothingGroup = null;
			OBJGroup? mergingGroup = null;
			uint materialIndex = 0;

			OBJObject? _current = null;
			OBJObject Current () {
				if ( _current is null ) {
					_current = new OBJObject( source );
					file.Objects.Add( _current );
					smoothingGroup?.Add( _current );
					mergingGroup?.Add( _current );
					foreach ( var i in activeGroups )
						i.Add( _current );
				}

				return _current;
			}
			var errors = file.ParsingErrors;

			string? takeNext ( ref string data ) {
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

			float[] parseFloats ( string data ) {
				return data.Split( ' ', StringSplitOptions.RemoveEmptyEntries ).Select( float.Parse ).ToArray();
			}

			uint[] parseUints ( string data ) {
				return data.Split( ' ', StringSplitOptions.RemoveEmptyEntries ).Select( uint.Parse ).ToArray();
			}

			uint?[][] parseMultiReferences ( string data ) {
				return data.Split( ' ', StringSplitOptions.RemoveEmptyEntries ).Select( x => x.Split( '/' ).Select( x => x == "" ? null : (uint?)uint.Parse( x ) ).ToArray() ).ToArray();
			}

			bool transformMultiIndicesNotnull ( int sourceSize, uint?[][] source, int index, out uint[] indices ) {
				if ( source.Any( x => x.Length > index && x[ index ] is 0 or null ) ) {
					indices = Array.Empty<uint>();
					return false;
				}
				indices = source.Select(
					x => x[ index ] < 0
					? (uint)sourceSize + (uint)x[ index ]!
					: (uint)x[ index ]! - 1
				).ToArray();
				return true;
			}

			bool transformMultiIndices ( int sourceSize, uint?[][] source, int index, out uint?[] indices ) {
				indices = source.Select(
					x => x.Length <= index
					? null
					: x[ index ] < 0
					? (uint)sourceSize + x[ index ]
					: x[ index ] - 1
				).ToArray();
				return !source.Any( x => x.Length > index && x[ index ] == 0 );
			}

			bool transformIndices ( int sourceSize, uint[] source, out uint[] indices ) {
				indices = source.Select(
					x => x < 0
					? (uint)sourceSize + x
					: x - 1
				).ToArray();
				return !source.Any( x => x == 0 );
			}

			CurveParameterData[]? parseCurveData ( ref string rest, string name, uint L, List<ParsingError> errors ) {
				List<CurveParameterData> data = new();
				while ( rest != "" ) {
					var a = takeNext( ref rest );
					var b = takeNext( ref rest );
					var c = takeNext( ref rest );

					if ( a == null || b == null || c == null ) {
						errors.Add( new( $"Expected {name} at L{L} to have a multiple of 3 parameters but it had some dangling values.", ParsingErrorSeverity.Issue ) );
						break;
					}

					data.Add( new CurveParameterData(
						float.Parse( a ),
						float.Parse( b ),
						uint.Parse( c )
					) );
				}

				if ( data.Count < 1 ) {
					errors.Add( new( $"Expected {name} at L{L} to have at least 3 values but it had 0.", ParsingErrorSeverity.Issue ) );
				}
				else {
					return data.ToArray();
				}
				return null;
			}

			uint L = 0;
			foreach ( var line in lines ) {
				var commentIndex = line.IndexOf( "#" );
				var rest = ( commentIndex == -1 ? line : line.Substring( 0, commentIndex ) ).Trim();
				L++;

				try {
					var type = takeNext( ref rest );
					if ( type is null or "" ) {
						continue;
					}

					// Vertex data
					else if ( type == "v" ) {
						var coords = parseFloats( rest );
						if ( coords.Length is < 3 or > 4 and not 7 ) {
							errors.Add( new( $"Expected vertice at L{L} to have between 3 and 4 coordinates or exactly 7, but it had {coords.Length}.", ParsingErrorSeverity.Error ) );
						}

						source.Vertices.Add( new Vector4( -coords.ElementAtOrDefault( 0 ), coords.ElementAtOrDefault( 1 ), coords.ElementAtOrDefault( 2 ), coords.Length > 3 ? coords[ 3 ] : 1 ) );

						if ( coords.Length == 7 ) {
							source.VerticeColours.Add( new Colour4( coords[ 4 ], coords[ 5 ], coords[ 6 ], 1 ) );
						}
						else {
							source.VerticeColours.Add( null );
						}
					}
					else if ( type == "vt" ) {
						var coords = parseFloats( rest );
						if ( coords.Length is < 1 or > 3 ) {
							errors.Add( new( $"Expected texture coordinate at L{L} to have between 1 and 3 coordinates, but it had {coords.Length}.", ParsingErrorSeverity.Issue ) );
						}

						source.TextureCoordinates.Add( new Vector3( coords.ElementAtOrDefault( 0 ), coords.ElementAtOrDefault( 1 ), coords.ElementAtOrDefault( 2 ) ) );
					}
					else if ( type == "vn" ) {
						var coords = parseFloats( rest );
						if ( coords.Length is not 3 ) {
							errors.Add( new( $"Expected vertex normal at L{L} to have 3 coordinates, but it had {coords.Length}.", ParsingErrorSeverity.Issue ) );
						}

						source.VerticeNormals.Add( new Vector3( -coords.ElementAtOrDefault( 0 ), coords.ElementAtOrDefault( 1 ), coords.ElementAtOrDefault( 2 ) ).Normalized() );
					}
					else if ( type == "vp" ) {
						var coords = parseFloats( rest );
						if ( coords.Length is < 1 or > 3 ) {
							errors.Add( new( $"Expected parameter-space vertice at L{L} to have between 1 and 3 coordinates, but it had {coords.Length}.", ParsingErrorSeverity.Error ) );
						}

						source.ParameterSpaceVertices.Add( new Vector3( coords.ElementAtOrDefault( 0 ), coords.ElementAtOrDefault( 1 ), coords.Length > 2 ? coords[ 2 ] : 1 ) );
					}
					else if ( type == "cstype" ) {
						var next = takeNext( ref rest );
						bool isRational = next == "rat";
						if ( isRational ) next = takeNext( ref rest );

						CurveType? curveType = next switch {
							"bmatrix" => CurveType.BasicMatrix,
							"bezier" => CurveType.Bezier,
							"bspline" => CurveType.Bspline,
							"cardinal" => CurveType.Cardinal,
							"taylor" => CurveType.Taylor,
							_ => null
						};

						if ( curveType is null ) {
							errors.Add( new( $"Invalid curve type at L{L}. Expected one of: bmatrix, bezier, bspline, cardinal or taylor but got {next}.", ParsingErrorSeverity.Issue ) );
							continue;
						}

						var cs = new CurveSurfaceData( curveType.Value, isRational );
						source.CurveSurfaces.Add( cs );
					}
					else if ( type == "deg" ) {
						var degrees = parseUints( rest );
						if ( degrees.Length is < 1 or > 2 ) {
							errors.Add( new( $"Expected degree at L{L} to have between 1 and 2 parameters, but it had {degrees.Length}.", ParsingErrorSeverity.Error ) );
						}

						source.DegreesU.Add( degrees[ 0 ] );
						if ( degrees.Length > 1 ) 
							source.DegreesV.Add( degrees[ 1 ] );
						else
							source.DegreesV.Add( null );
					}
					else if ( type == "bmat" ) {
						var d = takeNext( ref rest );
						Direction? direction = d switch {
							"u" => Direction.Horizontal,
							"v" => Direction.Vertical,
							_ => null
						};

						if ( direction is null ) {
							errors.Add( new( $"Invalid matrix direction at L{L}. Expected one of u or v, but got {d}.", ParsingErrorSeverity.Issue ) );
							continue;
						}

						var elements = parseFloats( rest );
						if ( direction == Direction.Horizontal )
							source.MatricesU.Add( elements );
						else
							source.MatricesV.Add( elements );
					}
					else if ( type == "step" ) {
						var steps = parseUints( rest );
						if ( steps.Length is < 1 or > 2 ) {
							errors.Add( new( $"Expected steps at L{L} to have between 1 and 2 parameters, but they had {steps.Length}.", ParsingErrorSeverity.Error ) );
						}

						source.StepsU.Add( steps[ 0 ] );
						if ( steps.Length > 1 ) 
							source.StepsV.Add( steps[ 1 ] );
						else
							source.StepsV.Add( null );
					}
					// Elements
					else if ( type == "p" ) {
						foreach ( var n in parseUints( rest ) ) {
							if ( n == 0 ) {
								errors.Add( new( $"Point at L{L} is malformed as it has invalid vertice indices declared.", ParsingErrorSeverity.Issue ) );
							}
							else {
								source.Points.Add( n < 0 ? (uint)source.Vertices.Count + n : n - 1 );
								Current().Points.Add( (uint)source.Points.Count - 1 );
							}
						}
					}
					else if ( type == "l" ) {
						var coords = parseMultiReferences( rest );

						if ( coords.Length < 2 ) {
							errors.Add( new( $"Expected line at L{L} to have at least 2 vertices, but it had {coords.Length}.", ParsingErrorSeverity.Issue ) );
						}

						if ( !transformMultiIndicesNotnull( source.Vertices.Count, coords, 0, out var vertices )
							|| !transformMultiIndices( source.TextureCoordinates.Count, coords, 1, out var tx )
						) {
							errors.Add( new( $"Line at L{L} is malformed as it has invalid vertice indices declared.", ParsingErrorSeverity.Issue ) );
						}
						else {
							source.Lines.Add( new LineData(
								vertices,
								tx
							) );
							Current().Lines.Add( (uint)source.Lines.Count - 1 );
						}
					}
					else if ( type is "f" or "fo" ) {
						var coords = parseMultiReferences( rest );

						if ( coords.Length < 3 ) {
							errors.Add( new( $"Expected face at L{L} to have at least 3 indices, but it had {coords.Length}.", ParsingErrorSeverity.Issue ) );
						}
						else if ( !transformMultiIndicesNotnull( source.Vertices.Count, coords, 0, out var vertices )
							|| !transformMultiIndices( source.TextureCoordinates.Count, coords, 1, out var tx )
							|| !transformMultiIndices( source.VerticeNormals.Count, coords, 2, out var nor )
						) {
							errors.Add( new( $"Face at L{L} is malformed as it has invalid vertice indices declared.", ParsingErrorSeverity.Issue ) );
						}
						else {
							source.Faces.Add( new FaceData( vertices, tx, nor, materialIndex ) );
							Current().Faces.Add( (uint)source.Faces.Count - 1 );

							if ( coords.Any( x => x.Length > 3 ) ) {
								errors.Add( new( $"Face at L{L} declared more than 3 indices per vertex, which is considered malformed input.", ParsingErrorSeverity.Issue ) );
							}
						}
					}
					else if ( type == "curv" ) {
						var a = takeNext( ref rest );
						var b = takeNext( ref rest );
						if ( a is null || b is null ) {
							errors.Add( new( $"Expected curve at L{L} to have a start and end parameter values.", ParsingErrorSeverity.Issue ) );
							continue;
						}
						var @params = parseUints( rest );

						if ( @params.Length < 2 ) {
							errors.Add( new( $"Expected curve at L{L} to have at least 2 vertices, but it had {@params.Length}.", ParsingErrorSeverity.Issue ) );
						}
						else {
							var start = float.Parse( a );
							var end = float.Parse( b );

							if ( !transformIndices( source.Vertices.Count, @params, out var vertices ) ) {
								errors.Add( new( $"Curve at L{L} is malformed as it has invalid vertice indices declared.", ParsingErrorSeverity.Issue ) );
							}

							source.Curves.Add( new CurveData( start, end, vertices ) );
						}
					}
					else if ( type == "curv2" ) {
						if ( !transformIndices( source.ParameterSpaceVertices.Count, parseUints( rest ), out var vertices ) ) {
							errors.Add( new( $"Curve at L{L} is malformed as it has invalid vertice indices declared.", ParsingErrorSeverity.Issue ) );
							continue;
						}

						if ( vertices.Length < 2 ) {
							errors.Add( new( $"Expected curve at L{L} to have at least 2 vertices, but it had {vertices.Length}.", ParsingErrorSeverity.Issue ) );
						}

						source.V2Curves.Add( new CurveV2Data( vertices ) );
					}
					else if ( type == "surf" ) {
						var s1 = takeNext( ref rest );
						var s2 = takeNext( ref rest );
						var t1 = takeNext( ref rest );
						var t2 = takeNext( ref rest );

						if ( s1 is null || s2 is null ) {
							errors.Add( new( $"Expected surface at L{L} to have a start and end parameter values for the U direction.", ParsingErrorSeverity.Issue ) );
							continue;
						}

						if ( t1 is null || t2 is null ) {
							errors.Add( new( $"Expected surface at L{L} to have a start and end parameter values for the V direction.", ParsingErrorSeverity.Issue ) );
							continue;
						}

						var indices = parseMultiReferences( rest );
						if ( !transformMultiIndicesNotnull( source.Vertices.Count, indices, 0, out var vertices )
							|| !transformMultiIndices( source.TextureCoordinates.Count, indices, 1, out var tx )
							|| !transformMultiIndices( source.VerticeNormals.Count, indices, 1, out var nor )
						) {
							errors.Add( new( $"Surface at L{L} is malformed as it has invalid vertice indices declared.", ParsingErrorSeverity.Issue ) );
						}
						else {
							source.Surfaces.Add( new SurfaceData(
								float.Parse( s1 ),
								float.Parse( s2 ),
								float.Parse( t1 ),
								float.Parse( t2 ),
								vertices,
								tx,
								nor
							) );
						}
					}
					// Free-form curve/surface body statements
					else if ( type == "parm" ) {
						var d = takeNext( ref rest );

						Direction? direction = d switch {
							"u" => Direction.Horizontal,
							"v" => Direction.Vertical,
							_ => null
						};

						if( direction is null ) {
							errors.Add( new( $"Invalid parameter direction at L{L}. Expected one of u or v, but got {d}.", ParsingErrorSeverity.Issue ) );
							continue;
						}

						var elements = parseFloats( rest );

						if ( elements.Length < 2 ) {
							errors.Add( new( $"Expected parameter at L{L} to have at least 2 values, but it had {elements.Length}.", ParsingErrorSeverity.Issue ) );
						}

						if ( direction == Direction.Horizontal )
							source.ParametersU.Add( elements );
						else
							source.ParametersV.Add( elements );
					}
					else if ( type == "trim" ) {
						if ( parseCurveData( ref rest, "trimming curve", L, errors ) is CurveParameterData[] data ) {
							source.Trims.Add( data );
						}
					}
					else if ( type == "hole" ) {
						if ( parseCurveData( ref rest, "hole trimming curve", L, errors ) is CurveParameterData[] data ) {
							source.Holes.Add( data );
						}
					}
					else if ( type == "scrv" ) {
						if ( parseCurveData( ref rest, "special trimming curve", L, errors ) is CurveParameterData[] data ) {
							source.SpecialCurves.Add( data );
						}
					}
					else if ( type == "sp" ) {
						if ( !transformIndices( source.Vertices.Count, parseUints( rest ), out var vertices ) ) {
							errors.Add( new( $"Special point at L{L} is malformed as it has invalid vertice indices declared.", ParsingErrorSeverity.Issue ) );
							continue;
						}

						if ( vertices.Length < 1 ) {
							errors.Add( new( $"Expected special point at L{L} to have at least 1 vertice, but it had {vertices.Length}.", ParsingErrorSeverity.Issue ) );
						}

						source.SpecialPoints.Add( vertices );
					}
					else if ( type == "end" ) {

					}
					// Connectivity between free-form surfaces
					else if ( type == "con" ) {
						var surf1 = takeNext( ref rest );
						var q01 = takeNext( ref rest );
						var q11 = takeNext( ref rest );
						var curv1 = takeNext( ref rest );
						var surf2 = takeNext( ref rest );
						var q02 = takeNext( ref rest );
						var q12 = takeNext( ref rest );
						var curv2 = takeNext( ref rest );

						if ( surf1 is null || surf2 is null || q01 is null || q11 is null || curv1 is null || curv2 is null || q02 is null || q12 is null ) {
							errors.Add( new( $"Expected connectivity data at L{L} to have 8 values.", ParsingErrorSeverity.Issue ) );
							continue;
						}

						source.Connections.Add( new ConnectivityData(
							uint.Parse( surf1 ),
							float.Parse( q01 ),
							float.Parse( q11 ),
							uint.Parse( curv1 ),
							uint.Parse( surf2 ),
							float.Parse( q02 ),
							float.Parse( q12 ),
							uint.Parse( curv2 )
						) );
					}
					// Grouping
					else if ( type == "g" ) {
						var names = rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries );

						activeGroups.Clear();
						foreach ( var i in names ) {
							var group = file.Groups.FirstOrDefault( x => x.Name == i );
							if ( group is null ) {
								file.Groups.Add( group = new OBJGroup( i ) );
							}
							activeGroups.Add( group );
						}
					}
					else if ( type == "s" ) {
						if ( rest is "off" or "0" or "" )
							smoothingGroup = null;
						else {
							var name = $"Smoothing Group {rest}";
							smoothingGroup = file.SmoothingGroups.FirstOrDefault( x => x.Name == name );
							if ( smoothingGroup is null ) {
								smoothingGroup = new OBJGroup( name, GroupType.Smoothing );
								file.SmoothingGroups.Add( smoothingGroup );
							}
						}
					}
					else if ( type == "mg" ) {
						var name = takeNext( ref rest );
						if ( name is "0" or "" )
							mergingGroup = null;
						else {
							name = $"Merging Group {rest}";
							mergingGroup = file.MergingGroups.FirstOrDefault( x => x.Name == name );
							if ( mergingGroup is null ) {
								mergingGroup = new MergingGroup( name, float.TryParse( rest, out var res ) ? res : 0 );
								file.MergingGroups.Add( mergingGroup );
							}
						}
					}
					else if ( type == "o" ) {
						_current = new OBJObject( source, rest );
						file.Objects.Add( _current );
						smoothingGroup?.Add( _current );
						mergingGroup?.Add( _current );
						foreach ( var i in activeGroups ) {
							i.Add( _current );
						}
					}
					// Display/render attributes
					else if ( type == "bevel" ) {
						errors.Add( new( $"{type} was declared at L{L}, but its not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
					}
					else if ( type == "c_interp" ) {
						errors.Add( new( $"{type} was declared at L{L}, but its not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
					}
					else if ( type == "d_interp" ) {
						errors.Add( new( $"{type} was declared at L{L}, but its not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
					}
					else if ( type == "lod" ) {
						errors.Add( new( $"{type} was declared at L{L}, but its not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
					}
					else if ( type == "usemtl" ) {
						var index = source.Materials.IndexOf( rest );
						if ( index == -1 ) {
							source.Materials.Add( rest );
							materialIndex = (uint)source.Materials.Count - 1;
						}
						else {
							materialIndex = (uint)index;
						}
					}
					else if ( type == "mtllib" ) {
						source.MTLFiles.AddRange( rest.Split( ' ', StringSplitOptions.RemoveEmptyEntries ).Select( x => new MTLFileReference( x ) ) );
					}
					else if ( type == "shadow_obj" ) {
						errors.Add( new( $"{type} was declared at L{L}, but its not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
					}
					else if ( type == "trace_obj" ) {
						errors.Add( new( $"{type} was declared at L{L}, but its not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
					}
					else if ( type == "ctech" ) {
						errors.Add( new( $"{type} was declared at L{L}, but its not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
					}
					else if ( type == "stech" ) {
						errors.Add( new( $"{type} was declared at L{L}, but its not implemented yet.", ParsingErrorSeverity.NotImplemented ) );
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

		public ImportedModelGroup CreateModelGroup () {
			var scene = new ImportedModelGroup( "Default Group" );
			Dictionary<MTLMaterial, ImportedMaterial> materials = new();

			foreach ( var i in Objects ) {
				var model = new ImportedModel( i.Name );

				foreach ( var group in i.Faces.Select( x => Data.Faces[ (int)x ] ).GroupBy( x => x.Material ) ) {
					var material = Data.FetchMaterial( (int)group.Key );

					ImportedMaterial mat;
					if ( material is null ) {
						mat = ImportedMaterial.Default;
					}
					else if ( !materials.TryGetValue( material, out mat! ) ) {
						materials.Add( material, mat = material.CreateMaterial() );
					}

					var mesh = new Mesh();

					Dictionary<uint, uint> vertexMap = new();
					
					uint mapIndices ( uint v, uint? t ) {
						if ( !vertexMap.TryGetValue( v, out var V ) ) {
							vertexMap.Add( v, V = (uint)mesh.Vertices.Count );
							mesh.Vertices.Add( Data.Vertices[ (int)v ].Xyz );
							mesh.TextureCoordinates.Add( t is null ? Vector2.Zero : Data.TextureCoordinates[ (int)t ].Xy );
						}

						return V;
					}

					foreach ( var face in group ) {
						var v1 = mapIndices( face.Vertices[ 0 ], face.TextureCoordinates[ 0 ] );
						var v2 = mapIndices( face.Vertices[ 1 ], face.TextureCoordinates[ 1 ] );

						foreach ( var (v,t) in face.Vertices.Zip( face.TextureCoordinates ).Skip( 2 ) ) {
							var v3 = mapIndices( v, t );

							mesh.Tris.Add( new IndexedFace( v1, v2, v3 ) );

							v2 = v3;
						}
					}

					model.Elements.Add( (mesh, mat) );
				}

				scene.Models.Add( model );
			}

			return scene;
		}
	}
}
