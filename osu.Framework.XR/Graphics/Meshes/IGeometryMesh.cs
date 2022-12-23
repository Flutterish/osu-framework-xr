using osu.Framework.XR.Maths;

namespace osu.Framework.XR.Graphics.Meshes;

public interface IGeometryMesh : IHasBoundingBox {
	uint VertexCount { get; }
	Vector3 GetVertexPosition ( uint index );
}

public interface IHasBoundingBox {
	AABox BoundingBox { get; }
}

public static class IGeometryMeshExtensions {
	public static IEnumerable<Vector3> EnumerateVertices ( this IGeometryMesh mesh ) {
		for ( uint i = 0; i < mesh.VertexCount; i++ ) {
			yield return mesh.GetVertexPosition( i );
		}
	}

	/// <summary>
	/// Fits a box around a flat mesh - that is one which can entirely fit on some plane. 
	/// This does *not* guarantee the twist of the box to be optimal
	/// </summary>
	/// <returns>
	/// A <see cref="Box3"/> if the mesh is flat (within a tolerance), <see langword="null"/> otherwise
	/// </returns>
	public static Box3? FitFlatMesh ( this IGeometryMesh mesh, float tolerance = 0.0001f ) {
		if ( mesh.VertexCount < 2 )
			return null;

		uint planeIndex = 0;
		var origin = mesh.GetVertexPosition(0);
		var lineA = mesh.GetVertexPosition(1) - origin;
		var lineB = mesh.GetVertexPosition(2) - origin;
		var plane = new Plane {
			Origin = origin,
			Normal = lineA.Cross( lineB ).Normalized()
		};

		bool tryResetPlane ( uint index ) {
			if ( planeIndex >= index || index + 2 >= mesh.VertexCount )
				return false;

			planeIndex = index;
			var origin = mesh.GetVertexPosition(index);
			var lineA = mesh.GetVertexPosition(index + 1) - origin;
			var lineB = mesh.GetVertexPosition(index + 2) - origin;
			plane = new Plane {
				Origin = origin,
				Normal = lineA.Cross( lineB ).Normalized()
			};
			return true;
		}

		for ( uint i = 0; i < mesh.VertexCount; ) {
			if ( plane.DistanceTo( mesh.GetVertexPosition(i) ) > tolerance ) {
				if ( !tryResetPlane( i ) )
					return null;
				i = 0;
			}
			i++;
		}

		var rotation = plane.Normal.LookRotation();
		var rotationInverse = rotation.Inverted();
		var bb = new AABox( mesh.EnumerateVertices().Select( x => rotationInverse.Apply( x ) ) );

		return new Box3 {
			Position = rotation.Apply( bb.Min ),
			Size = bb.Size,
			Rotation = rotation
		};
	}
}