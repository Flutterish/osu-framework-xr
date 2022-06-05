using osu.Framework.XR.Graphics.Buffers;
using osu.Framework.XR.Graphics.Vertices;

namespace osu.Framework.XR.Graphics.Meshes;

public class BasicMesh : Mesh, ITriangleMesh {
	new public ElementBuffer<uint> ElementBuffer => (ElementBuffer<uint>)base.ElementBuffer!;
	public VertexBuffer<TexturedNormal> VertexBuffer => (VertexBuffer<TexturedNormal>)VertexBuffers[0];

	public BasicMesh () : base( new ElementBuffer<uint>(), new VertexBuffer<TexturedNormal>() ) { }

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
}
