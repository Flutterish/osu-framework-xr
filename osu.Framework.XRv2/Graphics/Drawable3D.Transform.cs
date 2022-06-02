using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Layout;
using System.Runtime.CompilerServices;

namespace osu.Framework.XR.Graphics;

public partial class Drawable3D {
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	void trySet ( ref float field, ref float value ) {
		if ( field == value )
			return;

		field = value;
		localMatrix.Invalidate();
		matrix.Invalidate();
		Invalidate( Invalidation.DrawNode | Invalidation.DrawInfo );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	void trySet ( ref Vector3 field, ref Vector3 value ) {
		if ( field == value )
			return;

		field = value;
		localMatrix.Invalidate();
		matrix.Invalidate();
		Invalidate( Invalidation.DrawNode | Invalidation.DrawInfo );
	}

	Vector3 position;
	new public Vector3 Position {
		get => position;
		set => trySet( ref position, ref value );
	}
	new public float X {
		get => position.X;
		set => trySet( ref position.X, ref value );
	}
	new public float Y {
		get => position.Y;
		set => trySet( ref position.Y, ref value );
	}
	public float Z {
		get => position.Z;
		set => trySet( ref position.Z, ref value );
	}

	Vector3 scale = Vector3.One;
	new public Vector3 Scale {
		get => scale;
		set => trySet( ref scale, ref value );
	}
	public float ScaleX {
		get => scale.X;
		set => trySet( ref scale.X, ref value );
	}
	public float ScaleY {
		get => scale.Y;
		set => trySet( ref scale.Y, ref value );
	}
	public float ScaleZ {
		get => scale.Z;
		set => trySet( ref scale.Z, ref value );
	}

	Quaternion rotation = Quaternion.Identity;
	new public Quaternion Rotation {
		get => rotation;
		set {
			if ( rotation == value )
				return;

			rotation = value;
			localMatrix.Invalidate();
			matrix.Invalidate();
			Invalidate( Invalidation.DrawNode | Invalidation.DrawInfo );
		}
	}

	Vector3 offset;
	public Vector3 Offset {
		get => offset;
		set => trySet( ref offset, ref value );
	}
	public float OffsetX {
		get => offset.X;
		set => trySet( ref offset.X, ref value );
	}
	public float OffsetY {
		get => offset.Y;
		set => trySet( ref offset.Y, ref value );
	}
	public float OffsetZ {
		get => offset.Z;
		set => trySet( ref offset.Z, ref value );
	}

	protected override bool OnInvalidate ( Invalidation invalidation, InvalidationSource source )
		=> (invalidation & Invalidation.DrawInfo) != 0;

	Cached<Matrix4> localMatrix = new();
	Cached<Matrix4> matrix = new();

	public Matrix4 LocalMatrix {
		get {
			if ( !localMatrix.IsValid ) { // TODO combine into one operation
				localMatrix.Value = Matrix4.CreateTranslation( offset )
					* Matrix4.CreateScale( scale )
					* Matrix4.CreateFromQuaternion( rotation )
					* Matrix4.CreateTranslation( position );
			}

			return localMatrix.Value;
		}
	}

	public Matrix4 Matrix {
		get {
			if ( !matrix.IsValid ) {
				matrix.Value = parent != null
					? LocalMatrix * parent.Matrix
					: LocalMatrix;
			}

			return matrix.Value;
		}
	}

	public Vector3 Forward => ( Rotation * new Vector4( 0, 0, 1, 1 ) ).Xyz;
	public Vector3 Right => ( Rotation * new Vector4( 1, 0, 0, 1 ) ).Xyz;
	public Vector3 Up => ( Rotation * new Vector4( 0, 1, 0, 1 ) ).Xyz;
	public Vector3 Back => ( Rotation * new Vector4( 0, 0, -1, 1 ) ).Xyz;
	public Vector3 Left => ( Rotation * new Vector4( -1, 0, 0, 1 ) ).Xyz;
	public Vector3 Down => ( Rotation * new Vector4( 0, -1, 0, 1 ) ).Xyz;
}
