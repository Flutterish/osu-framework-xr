namespace osu.Framework.XR.Graphics.Rendering;

public class Camera : Drawable3D {
	public float FovY = MathF.PI / 2;
	public float NearPlaneDistance = 0.01f;
	public float FarPlaneDistance = 1000f;

	public Matrix4 GetProjectionMatrix ( float width, float height ) => Matrix.Inverted()
		* Matrix4.CreateScale( 1, 1, -1 )
		* Matrix4.CreatePerspectiveFieldOfView(
			FovY,
			width / height,
			NearPlaneDistance,
			FarPlaneDistance
		);

	/// <summary>
	/// Projects a given point to <0;width><0;height>. Returns false if the point is behind the camera.
	/// </summary>
	public bool Project ( Vector3 pos, float width, float height, out Vector2 proj ) {
		var p = new Vector4( pos, 1 ) * GetProjectionMatrix( width, height );
		proj = new Vector2(
			( p.X / p.W + 1 ) / 2 * width,
			( 1 - p.Y / p.W ) / 2 * height
		);

		return p.Z > 0;
	}

	/// <summary>
	/// Computes a normal vector pointing at a given screenspace position.
	/// </summary>
	public Vector3 DirectionOf ( Vector2 pos, float width, float height ) {
		var mat = GetProjectionMatrix( width, height ).Inverted();
		var vec = new Vector4( 
			pos.X / width * 2 - 1, 
			-(pos.Y / height * 2) + 1,
		1, 1 ) * mat;
		return vec.Xyz.Normalized();
	}
}
