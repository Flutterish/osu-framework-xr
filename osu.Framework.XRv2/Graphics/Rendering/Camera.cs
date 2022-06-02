namespace osu.Framework.XR.Graphics.Rendering;

public class Camera : Drawable3D {
	public float FovY = MathF.PI / 2;
	public float NearPlaneDistance = 0.01f;
	public float FarPlaneDistance = 1000f;

	public Matrix4 GetProjectionMatrix ( float width, float height ) => Matrix4.CreateTranslation( -Position )
		* Matrix4.CreateFromQuaternion( Rotation.Inverted() )
		* Matrix4.CreateScale( 1, 1, -1 )
		* Matrix4.CreatePerspectiveFieldOfView(
			FovY,
			width / height,
			NearPlaneDistance,
			FarPlaneDistance
		);
}
