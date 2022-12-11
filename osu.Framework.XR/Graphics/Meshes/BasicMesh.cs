using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Vertices;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

/// <summary>
/// A basic generic purpose triangle mesh with UVs and normals
/// </summary>
public class BasicMesh : Mesh, ITriangleMesh {
	new public ElementBuffer<uint> ElementBuffer => (ElementBuffer<uint>)base.ElementBuffer!;
	public List<uint> Indices => ElementBuffer.Indices;

	public VertexBuffer<TexturedNormal> VertexBuffer => (VertexBuffer<TexturedNormal>)VertexBuffers[0];
	public List<TexturedNormal> Vertices => VertexBuffer.Data;

	new public static MeshDescriptor Descriptor = new MeshDescriptor()
		.SetAttribute( 0, 0, MeshDescriptor.Position )
		.SetAttribute( 0, 1, MeshDescriptor.UV )
		.SetAttribute( 0, 2, MeshDescriptor.Normal );
	public BasicMesh () : base( new ElementBuffer<uint>(), new VertexBuffer<TexturedNormal>() ) {
		base.Descriptor = Descriptor;
	}

	uint IGeometryMesh.VertexCount => (uint)Vertices.Count;
	public int TriangleCount => Indices.Count / 3;
	public Vector3 GetVertexPosition ( uint index )
		=> VertexBuffer.Data[(int)index].Position;
	public (uint indexA, uint indexB, uint indexC) GetTriangleIndices ( int index )
		=> (Indices[index * 3], Indices[index * 3 + 1], Indices[index * 3 + 2]);

	public void RecalculateBoundingBox () {
		Vector3 min = new( float.PositiveInfinity );
		Vector3 max = new( float.NegativeInfinity );
		foreach ( var vert in Vertices ) {
			var v = vert.Position;
			if ( v.X > max.X )
				max.X = v.X;
			if ( v.X < min.X )
				min.X = v.X;
			if ( v.Y > max.Y )
				max.Y = v.Y;
			if ( v.Y < min.Y )
				min.Y = v.Y;
			if ( v.Z > max.Z )
				max.Z = v.Z;
			if ( v.Z < min.Z )
				min.Z = v.Z;
		}

		BoundingBox = new() { Min = min, Size = max - min };
	}
	public AABox BoundingBox { get; set; } = new() { Min = new( float.NegativeInfinity ), Size = new( float.PositiveInfinity ) };

	public void Clear () {
		ElementBuffer.Indices.Clear();
		VertexBuffer.Data.Clear();
	}

	public void AddTriangle ( TexturedNormal a, TexturedNormal b, TexturedNormal c, bool computeNormal = false ) {
		if ( computeNormal ) {
			var normal = Vector3.Cross( a.Position - b.Position, a.Position - c.Position ).Normalized();
			a.Normal = normal;
			b.Normal = normal;
			c.Normal = normal;
		}

		Vertices.Add( a );
		Vertices.Add( b );
		Vertices.Add( c );
		Indices.Add( (uint)( VertexBuffer.Data.Count - 3 ) );
		Indices.Add( (uint)( VertexBuffer.Data.Count - 2 ) );
		Indices.Add( (uint)( VertexBuffer.Data.Count - 1 ) );
	}
	public void AddFace ( uint a, uint b, uint c ) {
		Indices.Add( a );
		Indices.Add( b );
		Indices.Add( c );
	}

	public void AddCircle ( Vector3 origin, Vector3 normal, Vector3 forward, int segments ) {
		uint offset = (uint)VertexBuffer.Data.Count;
		normal.Normalize();

		Vertices.Add( new() { Position = origin, UV = new( 0.5f ) } );
		Vertices.Add( new() { Position = origin + forward, UV = new( 1, 0.5f ) } );
		for ( int i = 1; i < segments; i++ ) {
			var angle = (float)i / segments * MathF.PI * 2;
			Vertices.Add( new() {
				Position = origin + Quaternion.FromAxisAngle( normal, angle ).Apply( forward ),
				UV = new Vector2( 0.5f + MathF.Cos( angle ) / 2, 0.5f + MathF.Sin( angle ) / 2 )
			} );
			AddFace( offset, offset + (uint)i, offset + (uint)i + 1 );
		}
		AddFace( offset, (uint)( segments + offset ), offset + 1 );
	}

	public void AddQuad ( Quad3 quad )
		=> AddQuad( quad, new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 1, 0 ) );

	public void AddQuad ( Quad3 quad, Vector2 TL, Vector2 TR, Vector2 BL, Vector2 BR ) {
		var normal = Vector3.Cross( quad.TR - quad.TL, quad.TR - quad.BR ).Normalized();
		int offset = Vertices.Count;

		Vertices.Add( new() { Position = quad.TL, UV = TL, Normal = normal } );
		Vertices.Add( new() { Position = quad.TR, UV = TR, Normal = normal } );
		Vertices.Add( new() { Position = quad.BL, UV = BL, Normal = normal } );
		Vertices.Add( new() { Position = quad.BR, UV = BR, Normal = normal } );

		AddFace( (uint)offset, (uint)offset + 3, (uint)offset + 1 );
		AddFace( (uint)offset, (uint)offset + 3, (uint)offset + 2 );
	}

	public void AddQuad ( Vector3 origin, Vector3 direction, Vector3 up, float length, float width ) {
		AddQuad( new Quad3(
			origin + width / 2 * up,
			origin + width / 2 * up + direction * length,
			origin - width / 2 * up,
			origin - width / 2 * up + direction * length
		) );
	}

	public void AddCircularArc ( Vector3 normal, Vector3 forward, float angle, float innerRadius, float outerRadius, int? steps = null, Vector3? origin = null ) {
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

			Vertices.Add( new() { Position = inner } );
			Vertices.Add( new() { Position = outer } );

			return ((uint)Vertices.Count - 2, (uint)Vertices.Count - 1);
		}

		var (lastVerticeA, lastVerticeB) = addVertices( 0 );
		for ( int i = 1; i <= steps; i++ ) {
			var (a, b) = addVertices( deltaAngle * i );

			AddFace( lastVerticeA, lastVerticeB, b );
			AddFace( a, b, lastVerticeA );

			(lastVerticeA, lastVerticeB) = (a, b);
		}
	}

	public static BasicMesh MakeCube ( float sidelength ) {
		var v = sidelength / 2;

		BasicMesh cube = new();
		cube.Indices.AddRange( new uint[] {
			0,  1,  2,  2,  3,  0,
			4,  5,  6,  6,  7,  4,
			8,  9,  10, 10, 4,  8,
			11, 2,  12, 12, 13, 11,
			10, 14, 5,  5,  4,  10,
			3,  2,  11, 11, 15, 3
		} );
		cube.Vertices.AddRange( new TexturedNormal[] {
			new() { Position = new( -v, -v, -v ), UV = new( 0, 0 ) },
			new() { Position = new(  v, -v, -v ), UV = new( 1, 0 ) },
			new() { Position = new(  v,  v, -v ), UV = new( 1, 1 ) },
			new() { Position = new( -v,  v, -v ), UV = new( 0, 1 ) },
			new() { Position = new( -v, -v,  v ), UV = new( 0, 0 ) },
			new() { Position = new(  v, -v,  v ), UV = new( 1, 0 ) },
			new() { Position = new(  v,  v,  v ), UV = new( 1, 1 ) },
			new() { Position = new( -v,  v,  v ), UV = new( 0, 1 ) },
			new() { Position = new( -v,  v,  v ), UV = new( 1, 0 ) },
			new() { Position = new( -v,  v, -v ), UV = new( 1, 1 ) },
			new() { Position = new( -v, -v, -v ), UV = new( 0, 1 ) },
			new() { Position = new(  v,  v,  v ), UV = new( 1, 0 ) },
			new() { Position = new(  v, -v, -v ), UV = new( 0, 1 ) },
			new() { Position = new(  v, -v,  v ), UV = new( 0, 0 ) },
			new() { Position = new(  v, -v, -v ), UV = new( 1, 1 ) },
			new() { Position = new( -v,  v,  v ), UV = new( 0, 0 ) }
		} );
		cube.CreateFullUnsafeUpload().Enqueue();

		return cube;
	}

	public static BasicMesh MakeQuad ( float sidelength ) {
		var v = sidelength / 2;

		BasicMesh quad = new();
		quad.AddQuad( new Quad3(
			new Vector3(  v, -v, 0 ),
			new Vector3( -v, -v, 0 ),
			new Vector3(  v,  v, 0 ),
			new Vector3( -v,  v, 0 )
		) );
		quad.CreateFullUnsafeUpload().Enqueue();
		return quad;
	}

	static Dictionary<int, BasicMesh> staticMeshes = new();
	static BasicMesh getLazy ( int index, Func<BasicMesh> f ) {
		if ( !staticMeshes.TryGetValue( index, out var mesh ) )
			staticMeshes[index] = mesh = f();
		return mesh;
	}

	/// <summary>
	/// A cube whose corner coordinates are 1 or -1 ~ this means its slidelength is 2
	/// </summary>
	public static BasicMesh UnitCornerCube => getLazy( 0, () => MakeCube(2) );
	/// <summary>
	/// A quad (facing Z+) whose corner coordinates are 1 or -1 ~ this means its slidelength is 2
	/// </summary>
	public static BasicMesh UnitCornerQuad => getLazy( 1, () => MakeQuad(2) );
	/// <summary>
	/// A cube whose sidelength is 1
	/// </summary>
	public static BasicMesh UnitCube => getLazy( 2, () => MakeCube(1) );
	/// <summary>
	/// A quad (facing Z+) whose sidelength is 1
	/// </summary>
	public static BasicMesh UnitQuad => getLazy( 3, () => MakeQuad(1) );
}
