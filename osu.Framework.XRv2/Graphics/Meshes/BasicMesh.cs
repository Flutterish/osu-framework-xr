using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Vertices;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

public class BasicMesh : Mesh, ITriangleMesh {
	new public ElementBuffer<uint> ElementBuffer => (ElementBuffer<uint>)base.ElementBuffer!;
	public List<uint> Indices => ElementBuffer.Indices;

	public VertexBuffer<TexturedVertex> VertexBuffer => (VertexBuffer<TexturedVertex>)VertexBuffers[0];
	public List<TexturedVertex> Vertices => VertexBuffer.Data;

	new public static MeshDescriptor Descriptor = new MeshDescriptor()
		.SetAttribute( 0, 0, MeshDescriptor.Position )
		.SetAttribute( 0, 1, MeshDescriptor.UV );
	public BasicMesh () : base( new ElementBuffer<uint>(), new VertexBuffer<TexturedVertex>() ) {
		base.Descriptor = Descriptor;
	}

	public int TriangleCount => Indices.Count / 3;
	public Vector3 GetTriangleVertex ( uint index )
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

	public void AddTriangle ( TexturedVertex a, TexturedVertex b, TexturedVertex c ) {
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
		int offset = Vertices.Count;

		Vertices.Add( new() { Position = quad.TL, UV = TL } );
		Vertices.Add( new() { Position = quad.TR, UV = TR } );
		Vertices.Add( new() { Position = quad.BL, UV = BL } );
		Vertices.Add( new() { Position = quad.BR, UV = BR } );

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

	static BasicMesh () {
		UnitCube = new();
		UnitCube.Indices.AddRange( new uint[] {
			0,  1,  2,  2,  3,  0,
			4,  5,  6,  6,  7,  4,
			8,  9,  10, 10, 4,  8,
			11, 2,  12, 12, 13, 11,
			10, 14, 5,  5,  4,  10,
			3,  2,  11, 11, 15, 3
		} );
		UnitCube.Vertices.AddRange( new TexturedVertex[] {
			new() { Position = new( -1, -1, -1 ), UV = new( 0, 0 ) },
			new() { Position = new(  1, -1, -1 ), UV = new( 1, 0 ) },
			new() { Position = new(  1,  1, -1 ), UV = new( 1, 1 ) },
			new() { Position = new( -1,  1, -1 ), UV = new( 0, 1 ) },
			new() { Position = new( -1, -1,  1 ), UV = new( 0, 0 ) },
			new() { Position = new(  1, -1,  1 ), UV = new( 1, 0 ) },
			new() { Position = new(  1,  1,  1 ), UV = new( 1, 1 ) },
			new() { Position = new( -1,  1,  1 ), UV = new( 0, 1 ) },
			new() { Position = new( -1,  1,  1 ), UV = new( 1, 0 ) },
			new() { Position = new( -1,  1, -1 ), UV = new( 1, 1 ) },
			new() { Position = new( -1, -1, -1 ), UV = new( 0, 1 ) },
			new() { Position = new(  1,  1,  1 ), UV = new( 1, 0 ) },
			new() { Position = new(  1, -1, -1 ), UV = new( 0, 1 ) },
			new() { Position = new(  1, -1,  1 ), UV = new( 0, 0 ) },
			new() { Position = new(  1, -1, -1 ), UV = new( 1, 1 ) },
			new() { Position = new( -1,  1,  1 ), UV = new( 0, 0 ) }
		} );
		UnitCube.CreateFullUnsafeUpload().Enqueue();
	}

	public static readonly BasicMesh UnitCube;
}
