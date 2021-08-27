using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.XR.Allocation;
using osu.Framework.XR.Maths;
using osuTK;
using osuTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Framework.XR.Graphics {
	public class Mesh {
		public BindableList<Vector3> Vertices { get; } = new();
		public BindableList<Vector2> TextureCoordinates { get; } = new();
		public BindableList<IndexedFace> Tris { get; } = new();

		public readonly ReadonlyIndexer<int, Face> Faces;
		public ulong UpdateVersion { get; private set; } = 1;
		/// <summary>
		/// Whether this mesh is fully loaded and can be edited/used.
		/// </summary>
		public bool IsReady = true; // TODO instead of doing this, we should implement swap-meshes and prevent editing while the mesh is ready
		public Mesh () {
			Faces = new( index => {
				var indices = Tris[ index ];
				return new Face( Vertices[ (int)indices.A ], Vertices[ (int)indices.B ], Vertices[ (int)indices.C ] );
			} );
			Vertices.CollectionChanged += ( _, _ ) => UpdateVersion++;
			Tris.CollectionChanged += ( _, _ ) => UpdateVersion++;
			TextureCoordinates.CollectionChanged += ( _, _ ) => UpdateVersion++;
		}

		private AABox boundgingBox;
		private ulong boundingBoxUpdateVersion;
		/// <summary>
		/// A box bounding this mesh. This is cached.
		/// </summary>
		public AABox BoundingBox {
			get {
				if ( boundingBoxUpdateVersion == UpdateVersion || !IsReady ) return boundgingBox;
				boundingBoxUpdateVersion = UpdateVersion;
				if ( Vertices.Any() ) {
					boundgingBox = new AABox {
						Min = new Vector3(
							Vertices.Min( v => v.X ),
							Vertices.Min( v => v.Y ),
							Vertices.Min( v => v.Z )
						)
					};
					boundgingBox.Size = new Vector3(
						Vertices.Max( v => v.X ),
						Vertices.Max( v => v.Y ),
						Vertices.Max( v => v.Z )
					) - boundgingBox.Min;
				}
				else {
					boundgingBox = new AABox();
				}


				return boundgingBox;
			}
		}

		public static Mesh FromOBJFile ( string path )
			=> FromOBJ( File.ReadAllLines( path ) );
		public static Mesh FromOBJFile ( string path, List<MeshParsingError> errors )
			=> FromOBJ( File.ReadAllLines( path ), errors );
		public static Mesh FromOBJ ( string lines )
			=> FromOBJ( lines.Split( '\n' ) );
		public static Mesh FromOBJ ( string lines, List<MeshParsingError> errors )
			=> FromOBJ( lines.Split( '\n' ), errors );
		public static Mesh FromOBJ ( IEnumerable<string> lines ) {
			// TODO Merge( IEnumerable<Mesh> ) so we dont repeat here
			return MultipleFromOBJ( lines ).FirstOrDefault() ?? new();
		}
		public static Mesh FromOBJ ( IEnumerable<string> lines, List<MeshParsingError> errors ) {
			// TODO Merge( IEnumerable<Mesh> ) so we dont repeat here
			return MultipleFromOBJ( lines, errors ).FirstOrDefault() ?? new();
		}

		public static IEnumerable<Mesh> MultipleFromOBJFile ( string path )
			=> MultipleFromOBJ( File.ReadAllLines( path ) );
		public static IEnumerable<Mesh> MultipleFromOBJFile ( string path, List<MeshParsingError> errors )
			=> MultipleFromOBJ( File.ReadAllLines( path ), errors );

		public static IEnumerable<Mesh> MultipleFromOBJ ( string lines )
			=> MultipleFromOBJ( lines.Split( '\n' ) );
		public static IEnumerable<Mesh> MultipleFromOBJ ( string lines, List<MeshParsingError> errors )
			=> MultipleFromOBJ( lines.Split( '\n' ), errors );

		public static IEnumerable<Mesh> MultipleFromOBJ ( IEnumerable<string> lines ) {
			using var errors = ListPool<MeshParsingError>.Shared.Rent();
			var value = MultipleFromOBJ( lines, errors );

			if ( errors.Any( x => x.Severity.HasFlagFast( MeshParsingErrorSeverity.Hard ) ) ) throw new InvalidOperationException( string.Join( '\n', errors ) );
			return value;
		}

		public static IEnumerable<Mesh> MultipleFromOBJ ( IEnumerable<string> lines, List<MeshParsingError> errors ) { // TODO from .obj parsing. problem is that it corrects for RHS coords
			Mesh current = new();
			uint offset = 1;

			int L = 0;
			foreach ( var i in lines ) {
				var line = i.Trim();
				L++;

				if ( line.StartsWith( "v " ) ) {
					var coords = line.Substring( 2 ).Split( ' ', options: StringSplitOptions.RemoveEmptyEntries ).Select( x => float.Parse( x ) ).ToArray();
					if ( coords.Length is < 3 or ( > 4 and not 7 ) ) {
						errors.Add( new( $"Expected vertice at L{L} to have between 3 and 4 coordinates or exactly 7, but it had {coords.Length}.", MeshParsingErrorSeverity.Hard ) );
						current.Vertices.Add( new Vector3( coords.ElementAtOrDefault( 0 ), coords.ElementAtOrDefault( 1 ), coords.ElementAtOrDefault( 2 ) ) );
					}
					else {
						current.Vertices.Add( new Vector3( coords[ 0 ], coords[ 1 ], coords[ 2 ] ) );
					}

					if ( coords.Length == 7 ) {
						errors.Add( new( $"Vertice at L{L} specified a vertice color, but they are not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
					}
				}
				else if ( line.StartsWith( "vt " ) ) {
					var coords = line.Substring( 3 ).Split( ' ', options: StringSplitOptions.RemoveEmptyEntries ).Select( x => float.Parse( x ) ).ToArray();
					if ( coords.Length is < 1 or > 3 ) {
						errors.Add( new( $"Expected texture coordinate at L{L} to have between 1 and 3 coordinates, but it had {coords.Length}.", MeshParsingErrorSeverity.Soft ) );
						current.TextureCoordinates.Add( new Vector2( coords.ElementAtOrDefault( 0 ), coords.ElementAtOrDefault( 1 ) ) );
					}
					else {
						current.TextureCoordinates.Add( new Vector2( coords[ 0 ], coords.Length > 1 ? coords[ 1 ] : 0 ) );
					}
				}
				else if ( line.StartsWith( "vn " ) ) {
					var coords = line.Substring( 3 ).Split( ' ', options: StringSplitOptions.RemoveEmptyEntries ).Select( x => float.Parse( x ) ).ToArray();
					if ( coords.Length is not 3 ) {
						errors.Add( new( $"Expected vertex normal at L{L} to have 3 coordinates, but it had {coords.Length}.", MeshParsingErrorSeverity.Soft ) );
					}
					
					errors.Add( new( $"Vertex normal was declared at L{L}, but they are not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
				}
				else if ( line.StartsWith( "vp " ) ) {
					var coords = line.Substring( 3 ).Split( ' ', options: StringSplitOptions.RemoveEmptyEntries ).Select( x => float.Parse( x ) ).ToArray();
					if ( coords.Length is < 1 or > 3 ) {
						errors.Add( new( $"Expected free-form geometry vertice at L{L} to have between 1 and 3 coordinates, but it had {coords.Length}.", MeshParsingErrorSeverity.Hard ) );
					}
					errors.Add( new( $"Free-form geometry vertice was declared at L{L}, but they are not implemented yet.", MeshParsingErrorSeverity.HardNotImplemented ) );
				}
				else if ( line.StartsWith( "f " ) ) {
					var coords = line.Substring( 2 ).Split( ' ', options: StringSplitOptions.RemoveEmptyEntries )
						.Select( x => x.Split( '/' ).Select( y => y == "" ? null : (uint?)uint.Parse( y ) ).ToArray() ).ToArray();

					if ( coords.Length < 3 ) {
						errors.Add( new( $"Expected face at L{L} to have at least 3 indices, but it had {coords.Length}.", MeshParsingErrorSeverity.Soft ) );
					}
					else if ( coords.Any( x => x[ 0 ] is null ) ) {
						errors.Add( new( $"Face at L{L} is malformed as it has invalid vertice indices declared.", MeshParsingErrorSeverity.Soft ) );
					}
					else {
						if ( coords.Any( x => x.Length > 1 && x[ 1 ] is not null ) ) {
							errors.Add( new( $"Face at L{L} declared texture coordinate indices, but per-face texture coordinates are not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
						}
						if ( coords.Any( x => x.Length > 2 && x[ 2 ] is null ) ) {
							errors.Add( new( $"Face at L{L} declared normal coordinate indices, but per-face vertice normal coordinates are not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
						}
						if ( coords.Any( x => x.Length > 3 ) ) {
							errors.Add( new( $"Face at L{L} declared more than 3 indices per vertex, which is considered malformed input.", MeshParsingErrorSeverity.Soft ) );
						}
						
						if ( coords.Length > 3 ) {
							errors.Add( new( $"Face at L{L} declared {coords.Length} indices, but parsing this many is not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
						}
						else {
							current.Tris.Add( new( (uint)coords[ 0 ][ 0 ]! - offset, (uint)coords[ 1 ][ 0 ]! - offset, (uint)coords[ 2 ][ 0 ]! - offset ) );
						}
					}
				}
				else if ( line.StartsWith( "l " ) ) {
					var coords = line.Substring( 2 ).Split( ' ', options: StringSplitOptions.RemoveEmptyEntries ).Select( x => uint.Parse( x ) );
					
					errors.Add( new( $"Line segment was declared at L{L}, but they are not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
				}
				else if ( line.StartsWith( "mtllib " ) ) {
					var name = line.Substring( 7 );
					
					errors.Add( new( $"Material library was declared at L{L}, but they are not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
				}
				else if ( line.StartsWith( "usemtl " ) ) {
					var name = line.Substring( 7 );
					
					errors.Add( new( $"Material library usage was declared at L{L}, but they are not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
				}
				else if ( line.StartsWith( "o " ) ) {
					var name = line.Substring( 2 );

					if ( !current.IsEmpty ) yield return current;
					offset += (uint)current.Vertices.Count;
					current = new();
				}
				else if ( line.StartsWith( "g " ) ) {
					var name = line.Substring( 2 );
					
					errors.Add( new( $"Object group was declared at L{L}, but they are not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
				}
				else if ( line.StartsWith( "s " ) ) {
					var value = line.Substring( 2 );
					
					errors.Add( new( $"Shading style was declared at L{L}, but they are not implemented yet.", MeshParsingErrorSeverity.NotImplemented ) );
				}
			}

			if ( !current.IsEmpty ) yield return current;
		}

		public record MeshParsingError ( string Message, MeshParsingErrorSeverity Severity ) { }
		[Flags]
		public enum MeshParsingErrorSeverity {
			/// <summary>
			/// Does not break any dependencies.
			/// </summary>
			Soft = 0,
			/// <summary>
			/// Breaks some dependencies.
			/// </summary>
			Hard = 1,
			/// <summary>
			/// Does not break any dependencies, but does not work yet.
			/// </summary>
			NotImplemented = 2,
			/// <summary>
			/// Breaks some dependencies and does not work yet.
			/// </summary>
			HardNotImplemented = NotImplemented | Hard,
		}

		public bool IsEmpty => !Tris.Any();

		private void FillTextureCoordinates () {
			while ( TextureCoordinates.Count < Vertices.Count ) {
				TextureCoordinates.Add( Vector2.Zero );
			}
		}

		public int UploadToGPU ( int positionLocation, int uvLocation, int attributeBuffer, int elementBuffer, BufferUsageHint hint = BufferUsageHint.StaticDraw ) {
			if ( !IsReady ) {
				UpdateVersion++;
				return 0;
				//throw new InvalidOperationException( "This mesh is not avaialbe. This exception was probably caused by trying to edit a mesh which is attached to a drawable. Instead, you should create a new mesh and attach it after the work is done." );
			}

			FillTextureCoordinates();
			var vertices = new float[ Vertices.Count * 5 ];
			for ( int i = 0; i < Vertices.Count; i++ ) {
				vertices[ i * 5 ] = Vertices[ i ].X;
				vertices[ i * 5 + 1 ] = Vertices[ i ].Y;
				vertices[ i * 5 + 2 ] = Vertices[ i ].Z;
				vertices[ i * 5 + 3 ] = TextureCoordinates[ i ].X;
				vertices[ i * 5 + 4 ] = TextureCoordinates[ i ].Y;
			}
			var indices = new uint[ Tris.Count * 3 ];
			for ( int i = 0; i < Tris.Count; i++ ) {
				indices[ i * 3 ] = Tris[ i ].A;
				indices[ i * 3 + 1 ] = Tris[ i ].B;
				indices[ i * 3 + 2 ] = Tris[ i ].C;
			}

			GLWrapper.BindBuffer( osuTK.Graphics.ES30.BufferTarget.ArrayBuffer, attributeBuffer );
			GL.BufferData( BufferTarget.ArrayBuffer, vertices.Length * sizeof( float ), vertices, hint );
			GL.VertexAttribPointer( positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof( float ), 0 );
			GL.EnableVertexAttribArray( positionLocation );
			GL.VertexAttribPointer( uvLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof( float ), 3 * sizeof( float ) );
			GL.EnableVertexAttribArray( uvLocation );
			GLWrapper.BindBuffer( osuTK.Graphics.ES30.BufferTarget.ElementArrayBuffer, elementBuffer );
			GL.BufferData( BufferTarget.ElementArrayBuffer, indices.Length * sizeof( uint ), indices, hint );

			return indices.Length;
		}

		public void AddQuad ( Quad quad )
			=> AddQuad( quad, new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 1, 0 ) );

		public void AddQuad ( Quad quad, Vector2 TL, Vector2 TR, Vector2 BL, Vector2 BR ) {
			FillTextureCoordinates();
			int offset = Vertices.Count;

			Vertices.Add( quad.TL );
			Vertices.Add( quad.TR );
			Vertices.Add( quad.BL );
			Vertices.Add( quad.BR );

			Tris.Add( new( (uint)offset, (uint)offset + 3, (uint)offset + 1 ) );
			Tris.Add( new( (uint)offset, (uint)offset + 3, (uint)offset + 2 ) );
			TextureCoordinates.Add( TL );
			TextureCoordinates.Add( TR );
			TextureCoordinates.Add( BL );
			TextureCoordinates.Add( BR );
		}

		public void AddQuad ( Vector3 origin, Vector3 direction, Vector3 up, float length, float width ) {
			AddQuad( new Quad(
				origin + width / 2 * up,
				origin + width / 2 * up + direction * length,
				origin - width / 2 * up,
				origin - width / 2 * up + direction * length
			) );
		}

		public void AddCircle ( Vector3 origin, Vector3 normal, Vector3 forward, int segments ) {
			FillTextureCoordinates();
			uint offset = (uint)Vertices.Count;

			normal.Normalize();
			Vertices.Add( origin );
			Vertices.Add( origin + forward );
			for ( int i = 1; i < segments; i++ ) {
				var angle = (float)i / segments * MathF.PI * 2;
				Vertices.Add( origin + ( Quaternion.FromAxisAngle( normal, angle ) * new Vector4( forward, 1 ) ).Xyz );
				Tris.Add( new IndexedFace( offset, offset + (uint)i, offset + (uint)i + 1 ) );
			}
			Tris.Add( new IndexedFace( offset, (uint)( segments + offset ), offset + 1 ) );
		}

		public void AddCircularArc ( Vector3 normal, Vector3 forward, float angle, float innerRadius, float outerRadius, int? steps = null, Vector3? origin = null ) {
			FillTextureCoordinates();

			forward.Normalize();
			normal.Normalize();

			origin ??= Vector3.Zero;
			steps ??= (int)( angle / MathF.PI * 128 );
			if ( steps < 1 ) steps = 1;
			var deltaAngle = angle / steps.Value;

			(uint a, uint b) addVertices ( float angle ) {
				var direction = Quaternion.FromAxisAngle( normal, angle ).Apply( forward );
				var inner = innerRadius * direction + origin.Value;
				var outer = outerRadius * direction + origin.Value;

				Vertices.Add( inner );
				Vertices.Add( outer );

				return ( (uint)Vertices.Count - 2, (uint)Vertices.Count - 1 );
			}

			var (lastVerticeA, lastVerticeB) = addVertices( 0 );
			for ( int i = 1; i < steps; i++ ) {
				var (a, b) = addVertices( deltaAngle * i );

				Tris.Add(new IndexedFace(lastVerticeA, lastVerticeB, b));
				Tris.Add(new IndexedFace(a, b, lastVerticeA));

				(lastVerticeA, lastVerticeB) = (a, b);
			}
		}

		public void AddArcedPlane ( Vector3 up, Vector3 forward, float height, float radius, float angle, int? steps = null, Vector3? origin = null ) {
			FillTextureCoordinates();

			forward.Normalize();
			up.Normalize();

			origin ??= Vector3.Zero;
			steps ??= (int)( angle / MathF.PI * 128 );
			if ( steps < 1 ) steps = 1;
			var deltaAngle = angle / steps.Value;

			(uint a, uint b) addVertices ( float angle ) {
				var middle = Quaternion.FromAxisAngle( up, angle ).Apply( forward ) * radius + origin.Value;
				Vertices.Add( middle - up * height / 2 );
				Vertices.Add( middle + up * height / 2 );

				return ( (uint)Vertices.Count - 2, (uint)Vertices.Count - 1 );
			}

			var (lastVerticeA, lastVerticeB) = addVertices( 0 );
			for ( int i = 1; i < steps; i++ ) {
				var (a, b) = addVertices( deltaAngle * i );

				Tris.Add(new IndexedFace(lastVerticeA, lastVerticeB, b));
				Tris.Add(new IndexedFace(a, b, lastVerticeA));

				(lastVerticeA, lastVerticeB) = (a, b);
			}
		}

		public static Mesh UnitCube => FromOBJ(
			@"
			v  0.5  0.5  0.5
			v  0.5  0.5 -0.5
			v  0.5 -0.5  0.5
			v  0.5 -0.5 -0.5
			v -0.5  0.5  0.5
			v -0.5  0.5 -0.5
			v -0.5 -0.5  0.5
			v -0.5 -0.5 -0.5

			f 5 8 6
			f 5 8 7
			f 3 2 1
			f 3 2 4
			f 5 3 7
			f 5 3 1
			f 6 4 2
			f 6 4 8
			f 5 2 6
			f 5 2 1
			f 8 3 7
			f 8 3 4
			"
		);

		public static Mesh XZPlane ( float sizeX, float sizeZ ) {
			Mesh mesh = new();
			mesh.AddQuad(
				new Quad(
					new Vector3( -sizeX / 2, 0, sizeZ / 2 ),
					new Vector3( sizeX / 2, 0, sizeZ / 2 ),
					new Vector3( -sizeX / 2, 0, -sizeZ / 2 ),
					new Vector3( sizeX / 2, 0, -sizeZ / 2 )
				)
			);
			return mesh;
		}

		public static Mesh XYPlane ( float sizeX, float sizeY ) {
			Mesh mesh = new();
			mesh.AddQuad(
				new Quad(
					new Vector3( -sizeX / 2, sizeY / 2, 0 ),
					new Vector3( sizeX / 2, sizeY / 2, 0 ),
					new Vector3( -sizeX / 2, -sizeY / 2, 0 ),
					new Vector3( sizeX / 2, -sizeY / 2, 0 )
				)
			);
			return mesh;
		}

		public void Clear () {
			Vertices.Clear();
			TextureCoordinates.Clear();
			Tris.Clear();
		}
	}

	public struct IndexedFace {
		public uint A;
		public uint B;
		public uint C;

		public IndexedFace ( uint a, uint b, uint c ) {
			A = a;
			B = b;
			C = c;
		}
	}

	public struct Face {
		public Vector3 A;
		public Vector3 B;
		public Vector3 C;

		public Face ( Vector3 a, Vector3 b, Vector3 c ) {
			A = a;
			B = b;
			C = c;
		}

		public static Face operator * ( Matrix4x4 matrix, Face face ) {
			return new Face(
				( matrix * new Vector4( face.A, 1 ) ).Xyz,
				( matrix * new Vector4( face.B, 1 ) ).Xyz,
				( matrix * new Vector4( face.C, 1 ) ).Xyz
			);
		}
	}
}
