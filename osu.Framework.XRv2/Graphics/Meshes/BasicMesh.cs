using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Vertices;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

public class BasicMesh : Mesh, ITriangleMesh {
	new public ElementBuffer<uint> ElementBuffer => (ElementBuffer<uint>)base.ElementBuffer!;
	public List<uint> Indices => ElementBuffer.Indices;

	public VertexBuffer<TexturedVertex> VertexBuffer => (VertexBuffer<TexturedVertex>)VertexBuffers[0];
	public List<TexturedVertex> Vertices => VertexBuffer.Data;

	public BasicMesh () : base( new ElementBuffer<uint>(), new VertexBuffer<TexturedVertex>() ) { }

	public IEnumerable<(uint indexA, uint indexB, uint indexC)> TriangleIndices {
		get {
			var indices = ElementBuffer.Indices;
			for ( int i = 0; i < indices.Count; i += 3 ) {
				yield return ( indices[i], indices[i + 1], indices[i + 2] );
			}
		}
	}

	public Vector3 GetTriangleVertex ( uint index )
		=> VertexBuffer.Data[(int)index].Position;

	public void Clear () {
		ElementBuffer.Indices.Clear();
		VertexBuffer.Data.Clear();
	}

	public void AddTriangle ( TexturedVertex a, TexturedVertex b, TexturedVertex c ) {
		Vertices.Add( a );
		Vertices.Add( b );
		Vertices.Add( c );
		Indices.Add( (uint)(VertexBuffer.Data.Count - 3) );
		Indices.Add( (uint)(VertexBuffer.Data.Count - 2) );
		Indices.Add( (uint)(VertexBuffer.Data.Count - 1) );
	}
	public void AddFace ( uint a, uint b, uint c ) {
		Indices.Add( a );
		Indices.Add( b );
		Indices.Add( c );
	}

	public void AddCircle ( Vector3 origin, Vector3 normal, Vector3 forward, int segments ) {
		uint offset = (uint)VertexBuffer.Data.Count;
		normal.Normalize();

		Vertices.Add( new() { Position = origin, UV = new(0.5f) } );
		Vertices.Add( new() { Position = origin + forward, UV = new( 1, 0.5f ) } );
		for ( int i = 1; i < segments; i++ ) {
			var angle = (float)i / segments * MathF.PI * 2;
			Vertices.Add( new() { 
				Position = origin + Quaternion.FromAxisAngle( normal, angle ).Apply( forward ),
				UV = new Vector2(0.5f + MathF.Cos(angle) / 2, 0.5f + MathF.Sin( angle ) / 2 )
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
}
