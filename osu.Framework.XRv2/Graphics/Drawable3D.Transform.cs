using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Layout;
using System.Runtime.CompilerServices;

namespace osu.Framework.XR.Graphics;

public interface IHasMatrix {
	Matrix4 Matrix { get; }
}

public partial class Drawable3D : IHasMatrix {
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	void trySet ( ref float field, ref float value ) {
		if ( field == value )
			return;

		field = value;
		TryInvalidateMatrix();
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	void trySet ( ref Vector3 field, ref Vector3 value ) {
		if ( field == value )
			return;

		field = value;
		TryInvalidateMatrix();
	}

	public void TryInvalidateMatrix () {
		if ( localMatrixCache.Invalidate() ) {
			InvalidateMatrix();
		}
	}
	protected virtual void InvalidateMatrix () {
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
			localMatrixCache.Invalidate();
			matrix.Invalidate();
			Invalidate( Invalidation.DrawNode | Invalidation.DrawInfo );
		}
	}

	public Vector3 EulerRotation {
		get => rotation.ToEuler();
		set => rotation = Quaternion.FromEulerAngles( value );
	}
	public float EulerX {
		get => EulerRotation.X;
		set => rotation = Quaternion.FromEulerAngles( EulerRotation with { X = value } );
	}
	public float EulerY {
		get => EulerRotation.Y;
		set => rotation = Quaternion.FromEulerAngles( EulerRotation with { Y = value } );
	}
	public float EulerZ {
		get => EulerRotation.Z;
		set => rotation = Quaternion.FromEulerAngles( EulerRotation with { Z = value } );
	}

	Vector3 origin;
	new public Vector3 Origin {
		get => origin;
		set => trySet( ref origin, ref value );
	}
	public float OriginX {
		get => origin.X;
		set => trySet( ref origin.X, ref value );
	}
	public float OriginY {
		get => origin.Y;
		set => trySet( ref origin.Y, ref value );
	}
	public float OriginZ {
		get => origin.Z;
		set => trySet( ref origin.Z, ref value );
	}

	protected override bool OnInvalidate ( Invalidation invalidation, InvalidationSource source )
		=> ( invalidation & Invalidation.DrawInfo ) != 0;

	Cached localMatrixCache = new();
	Cached<Matrix4> matrix = new();

	Matrix4 localMatrix;
	public Matrix4 LocalMatrix {
		get {
			if ( !localMatrixCache.IsValid ) { // TODO combine into one operation (or just dont do the multiplication on known 0-cells)
				localMatrix = Matrix4.CreateTranslation( -origin );
				Matrix4 temp;
				if ( scale != Vector3.One ) {
					temp = Matrix4.CreateScale( scale );
					Matrix4.Mult( ref localMatrix, ref temp, out localMatrix );
				}
				if ( rotation != Quaternion.Identity ) {
					temp = Matrix4.CreateFromQuaternion( rotation );
					Matrix4.Mult( ref localMatrix, ref temp, out localMatrix );
				}
				if ( position != Vector3.Zero ) {
					temp = Matrix4.CreateTranslation( position );
					Matrix4.Mult( ref localMatrix, ref temp, out localMatrix );
				}

				localMatrixCache.Validate();
			}

			return localMatrix;
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
