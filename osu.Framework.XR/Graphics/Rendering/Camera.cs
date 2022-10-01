namespace osu.Framework.XR.Graphics.Rendering;

public class Camera : Drawable3D {
	public FovType FovType = FovType.Y;
	public float FovY = MathF.PI / 2;
	public float FovX = MathF.PI / 2;
	public float NearPlaneDistance = 0.01f;
	public float FarPlaneDistance = 1000f;

	public Matrix4 GetProjectionMatrix ( float width, float height ) {
		var mat = Matrix.Inverted()
		* Matrix4.CreateScale( 1, 1, -1 );

		Matrix4 fov;
		void useFovY () {
			fov = Matrix4.CreatePerspectiveFieldOfView(
				FovY,
				width / height,
				NearPlaneDistance,
				FarPlaneDistance
			);
		}
		void useFovX () {
			float right = NearPlaneDistance * MathF.Tan( FovX / 2 );
			var aspect = height / width;
			Matrix4.CreatePerspectiveOffCenter( -right, right, -right * aspect, right * aspect, NearPlaneDistance, FarPlaneDistance, out fov );
		}
		if ( FovType is FovType.Y ) {
			useFovY();
		}
		else if ( FovType is FovType.X ) {
			useFovX();
		}
		else if ( FovType is FovType.Max ) {
			float targetWidth = NearPlaneDistance * MathF.Tan( FovX / 2 );
			float targetHeight = NearPlaneDistance * MathF.Tan( FovY / 2 );
			if ( targetWidth / targetHeight > width / height ) {
				useFovY();
			}
			else {
				useFovX();
			}
		}
		else if ( FovType is FovType.Min ) {
			float targetWidth = NearPlaneDistance * MathF.Tan( FovX / 2 );
			float targetHeight = NearPlaneDistance * MathF.Tan( FovY / 2 );
			if ( targetWidth / targetHeight > width / height ) {
				useFovX();
			}
			else {
				useFovY();
			}
		}
		else {
			float right = NearPlaneDistance * MathF.Tan( FovX / 2 );
			float top = NearPlaneDistance * MathF.Tan( FovY / 2 );
			Matrix4.CreatePerspectiveOffCenter( -right, right, -top, top, NearPlaneDistance, FarPlaneDistance, out fov );
		}

		return mat * fov;
	}

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
	/// Projects a given point to <0;width><0;height>
	/// </summary>
	public Vector3 Project ( Vector3 pos, float width, float height ) {
		var p = new Vector4( pos, 1 ) * GetProjectionMatrix( width, height );
		return new Vector3(
			( p.X / p.W + 1 ) / 2 * width,
			( 1 - p.Y / p.W ) / 2 * height,
			p.Z
		);
	}

	/// <summary>
	/// Computes a normal vector pointing at a given screenspace position.
	/// </summary>
	public Vector3 DirectionOf ( Vector2 pos, float width, float height ) {
		var mat = GetProjectionMatrix( width, height ).Inverted();
		var vec = new Vector4(
			pos.X / width * 2 - 1,
			-( pos.Y / height * 2 ) + 1,
		1, 1 ) * mat;
		return vec.Xyz.Normalized();
	}
}

public enum FovType {
	/// <summary>
	/// Fov y is fixed while x changes to match aspect ratio
	/// </summary>
	Y,
	/// <summary>
	/// Fov x is fixed while y changes to match aspect ratio
	/// </summary>
	X,
	/// <summary>
	/// Fov x and y will be at most FovX and FovY repectively
	/// </summary>
	Max,
	/// <summary>
	/// Fov x and y will be at least FovX and FovY repectively
	/// </summary>
	Min,
	/// <summary>
	/// Fov x and y will be exactly FovX and FovY repectively
	/// </summary>
	Stretch
}