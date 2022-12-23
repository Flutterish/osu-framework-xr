using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

/// <inheritdoc/>
public class TransformedTriangleMesh : TransformedTriangleMesh<ITriangleMesh> {
	public TransformedTriangleMesh ( ITriangleMesh mesh, Func<Matrix4> matrixGetter ) : base(
		mesh,
		getTriangleCount,
		getVertexCount,
		getTriangle,
		getVertex,
		matrixGetter
	) { }

	static Func<ITriangleMesh, int> getTriangleCount = m => m.TriangleCount;
	static Func<ITriangleMesh, uint> getVertexCount = m => m.VertexCount;
	static Func<ITriangleMesh, int, (uint indexA, uint indexB, uint indexC)> getTriangle = ( m, i ) => m.GetTriangleIndices( i );
	static Func<ITriangleMesh, uint, Vector3> getVertex = ( m, i ) => m.GetVertexPosition( i );
}

/// <inheritdoc/>
public class TransformedGeometryMesh : TransformedTriangleMesh<IGeometryMesh> {
	public TransformedGeometryMesh ( IGeometryMesh mesh, Func<Matrix4> matrixGetter ) : base(
		mesh,
		getTriangleCount,
		getVertexCount,
		getTriangle,
		getVertex,
		matrixGetter
	) { }

	static Func<IGeometryMesh, int> getTriangleCount = m => 0;
	static Func<IGeometryMesh, uint> getVertexCount = m => m.VertexCount;
	static Func<IGeometryMesh, int, (uint indexA, uint indexB, uint indexC)> getTriangle = ( m, i ) => (0, 0, 0);
	static Func<IGeometryMesh, uint, Vector3> getVertex = ( m, i ) => m.GetVertexPosition( i );
}

/// <summary>
/// A cached mesh transformed by a matrix
/// </summary>
/// <remarks>
/// If the underlying mesh has been modified, you need to invalidate the appropriate cache in this object manually
/// </remarks>
public class TransformedTriangleMesh<T> : ITriangleMesh {
	Func<T, int> getTriangleCount;
	Func<T, uint> getVertexCount;
	Func<T, int, (uint indexA, uint indexB, uint indexC)> getTriangle;
	Func<T, uint, Vector3> getVertex;
	T mesh;
	public T Mesh {
		get => mesh;
		set {
			if ( ReferenceEquals( mesh, value ) )
				return;

			mesh = value;
			InvalidateAll();
		}
	}

	public void InvalidateMatrix () {
		matrix = null;
		vertices.Clear();
	}
	Func<Matrix4> matrixGetter;
	Matrix4? matrix;
	public Matrix4 Matrix => matrix ??= matrixGetter();

	public TransformedTriangleMesh (
		T mesh,
		Func<T, int> getTriangleCount, 
		Func<T, uint> getVertexCount, 
		Func<T, int, (uint indexA, uint indexB, uint indexC)> getTriangle, 
		Func<T, uint, Vector3> getVertex,
		Func<Matrix4> matrixGetter )
	{
		this.matrixGetter = matrixGetter;
		this.getTriangleCount = getTriangleCount;
		this.getVertexCount = getVertexCount;
		this.getTriangle = getTriangle;
		this.getVertex = getVertex;
		this.mesh = mesh;
	}

	public void InvalidateAll () {
		InvalidateTriangles();
		InvalidateVertices();
	}

	int? triangleCount;
	public int TriangleCount => triangleCount ??= getTriangleCount( mesh );

	public void InvalidateTriangleCount () {
		triangleCount = null;
	}

	public void InvalidateTriangles () {
		InvalidateTriangleCount();
		indices.Clear();
	}

	public void InvalidateTriangles ( Range range ) {
		var count = TriangleCount;

		var start = range.Start.IsFromEnd ? ( count - range.Start.Value ) : range.Start.Value;
		var end = range.End.IsFromEnd ? ( count - range.End.Value ) : range.End.Value;
		start = Math.Min( indices.Count, start );
		end = Math.Min( indices.Count, end );
		for ( int i = start; i < end; i++ ) {
			indices[i] = null;
		}
	}

	List<(uint indexA, uint indexB, uint indexC)?> indices = new();
	public (uint indexA, uint indexB, uint indexC) GetTriangleIndices ( int index ) {
		while ( indices.Count <= index ) {
			indices.Add( null );
		}

		return indices[index] ??= getTriangle( mesh, index );
	}

	public void InvalidateVertexCount () {
		vertexCount = null;
		boundingBox = null;
	}

	public void InvalidateVertices () {
		InvalidateVertexCount();
		vertices.Clear();
	}

	public void InvalidateVertices ( Range range ) {
		if ( vertexCount is not uint count )
			vertexCount = count = getVertexCount( mesh );

		var start = range.Start.IsFromEnd ? ( count - range.Start.Value ) : range.Start.Value;
		var end = range.End.IsFromEnd ? ( count - range.End.Value ) : range.End.Value;
		start = Math.Min( vertices.Count, start );
		end = Math.Min( vertices.Count, end );
		for ( int i = (int)start; i < end; i++ ) {
			vertices[i] = null;
		}
	}

	uint? vertexCount;
	public uint VertexCount => vertexCount ??= getVertexCount( mesh );

	List<Vector3?> vertices = new();
	public Vector3 GetVertexPosition ( uint index ) {
		while ( vertices.Count <= index ) {
			vertices.Add( null );
		}

		return vertices[(int)index] ??= Matrix.Apply( getVertex( mesh, index ) );
	}

	AABox? boundingBox;
	public AABox BoundingBox {
		get {
			if ( boundingBox is not AABox box ) {
				var count = VertexCount;

				Vector3 min = new( float.PositiveInfinity );
				Vector3 max = new( float.NegativeInfinity );
				for ( uint i = 0; i < count; i++ ) {
					var v = getVertex( mesh, i );
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

				boundingBox = box = new() { Min = min, Size = max - min };
			}

			return box * Matrix;
		}
	}
}
