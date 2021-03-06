using osuTK;
using System;
using System.Runtime.InteropServices;
using Valve.VR;

namespace osu.Framework.XR.Maths {
	/// <summary>
	/// Matrix4x4 is a copy of osuTK.Matrix4 that actually works on the CPU
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct Matrix4x4 : IEquatable<Matrix4x4> {
		public Vector4 Row0 => new Vector4( M00, M10, M20, M30 );
		public Vector4 Row1 => new Vector4( M01, M11, M21, M31 );
		public Vector4 Row2 => new Vector4( M02, M12, M22, M32 );
		public Vector4 Row3 => new Vector4( M03, M13, M23, M33 );

		public Vector4 Column0 {
			get => new Vector4( M00, M01, M02, M03 );
			set {
				M00 = value.X;
				M01 = value.Y;
				M02 = value.Z;
				M03 = value.W;
			}
		}
		public Vector4 Column1 {
			get => new Vector4( M10, M11, M12, M13 );
			set {
				M10 = value.X;
				M11 = value.Y;
				M12 = value.Z;
				M13 = value.W;
			}
		}
		public Vector4 Column2 {
			get => new Vector4( M20, M21, M22, M23 );
			set {
				M20 = value.X;
				M21 = value.Y;
				M22 = value.Z;
				M23 = value.W;
			}
		}
		public Vector4 Column3 {
			get => new Vector4( M30, M31, M32, M33 );
			set {
				M30 = value.X;
				M31 = value.Y;
				M32 = value.Z;
				M33 = value.W;
			}
		}

		public float M00;
		public float M10;
		public float M20;
		public float M30;

		public float M01;
		public float M11;
		public float M21;
		public float M31;

		public float M02;
		public float M12;
		public float M22;
		public float M32;

		public float M03;
		public float M13;
		public float M23;
		public float M33;

		public Matrix4x4 (
			float m00, float m10, float m20, float m30,
			float m01, float m11, float m21, float m31,
			float m02, float m12, float m22, float m32,
			float m03, float m13, float m23, float m33 ) {
			M00 = m00; M10 = m10; M20 = m20; M30 = m30;
			M01 = m01; M11 = m11; M21 = m21; M31 = m31;
			M02 = m02; M12 = m12; M22 = m22; M32 = m32;
			M03 = m03; M13 = m13; M23 = m23; M33 = m33;
		}

		public float this[ int x, int y ] {
			get {
				switch ( x ) {
					case 0:
						switch ( y ) {
							case 0: return M00;
							case 1: return M01;
							case 2: return M02;
							case 3: return M03;
							default: throw new ArgumentOutOfRangeException( $"Y has to be in <0;3> but was {y}" );
						}
					case 1:
						switch ( y ) {
							case 0: return M10;
							case 1: return M11;
							case 2: return M12;
							case 3: return M13;
							default: throw new ArgumentOutOfRangeException( $"Y has to be in <0;3> but was {y}" );
						}
					case 2:
						switch ( y ) {
							case 0: return M20;
							case 1: return M21;
							case 2: return M22;
							case 3: return M23;
							default: throw new ArgumentOutOfRangeException( $"Y has to be in <0;3> but was {y}" );
						}
					case 3:
						switch ( y ) {
							case 0: return M30;
							case 1: return M31;
							case 2: return M32;
							case 3: return M33;
							default: throw new ArgumentOutOfRangeException( $"Y has to be in <0;3> but was {y}" );
						}
					default: throw new ArgumentOutOfRangeException( $"X has to be in <0;3> but was {x}" );
				};
			}
			set {
				switch ( x ) {
					case 0:
						switch ( y ) {
							case 0: M00 = value; return;
							case 1: M01 = value; return;
							case 2: M02 = value; return;
							case 3: M03 = value; return;
							default: throw new ArgumentOutOfRangeException( $"Y has to be in <0;3> but was {y}" );
						}
					case 1:
						switch ( y ) {
							case 0: M10 = value; return;
							case 1: M11 = value; return;
							case 2: M12 = value; return;
							case 3: M13 = value; return;
							default: throw new ArgumentOutOfRangeException( $"Y has to be in <0;3> but was {y}" );
						}
					case 2:
						switch ( y ) {
							case 0: M20 = value; return;
							case 1: M21 = value; return;
							case 2: M22 = value; return;
							case 3: M23 = value; return;
							default: throw new ArgumentOutOfRangeException( $"Y has to be in <0;3> but was {y}" );
						}
					case 3:
						switch ( y ) {
							case 0: M30 = value; return;
							case 1: M31 = value; return;
							case 2: M32 = value; return;
							case 3: M33 = value; return;
							default: throw new ArgumentOutOfRangeException( $"Y has to be in <0;3> but was {y}" );
						}
					default: throw new ArgumentOutOfRangeException( $"X has to be in <0;3> but was {x}" );
				};
			}
		}

		public static Matrix4x4 Identity {
			get => new Matrix4x4(
				1, 0, 0, 0,
				0, 1, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1
			);
		}

		public static Matrix4x4 CreateTranslation ( Vector3 offset ) {
			return new Matrix4x4(
				1, 0, 0, offset.X,
				0, 1, 0, offset.Y,
				0, 0, 1, offset.Z,
				0, 0, 0, 1
			);
		}

		public static Matrix4x4 CreateTranslation ( float x = 0, float y = 0, float z = 0 ) {
			return new Matrix4x4(
				1, 0, 0, x,
				0, 1, 0, y,
				0, 0, 1, z,
				0, 0, 0, 1
			);
		}

		public static Matrix4x4 CreateScale ( Vector3 scale ) {
			return new Matrix4x4(
				scale.X, 0, 0, 0,
				0, scale.Y, 0, 0,
				0, 0, scale.Z, 0,
				0, 0, 0, 1
			);
		}

		public static Matrix4x4 CreateScale ( float x = 1, float y = 1, float z = 1 ) {
			return new Matrix4x4(
				x, 0, 0, 0,
				0, y, 0, 0,
				0, 0, z, 0,
				0, 0, 0, 1
			);
		}

		public static Matrix4x4 CreateRotation ( Quaternion q ) {
			var x4 = Matrix4.CreateFromQuaternion( q );
			return (Matrix4x4)x4; // I don't actually know how to do this lol
		}

		public static Matrix4x4 operator * ( Matrix4x4 l, Matrix4x4 r ) {
			/*
			r.M00,                                                         r.M10,                                                         r.M20,                                                         r.M30
			r.M01,                                                         r.M11,                                                         r.M21,                                                         r.M31
			r.M02,                                                         r.M12,                                                         r.M22,                                                         r.M32
			r.M03,                                                         r.M13,                                                         r.M23,                                                         r.M33 */
			return new Matrix4x4(
			/* l.M00, l.M10, l.M20, l.M30 */    l.M00 * r.M00 + l.M10 * r.M01 + l.M20 * r.M02 + l.M30 * r.M03, l.M00 * r.M10 + l.M10 * r.M11 + l.M20 * r.M12 + l.M30 * r.M13, l.M00 * r.M20 + l.M10 * r.M21 + l.M20 * r.M22 + l.M30 * r.M23, l.M00 * r.M30 + l.M10 * r.M31 + l.M20 * r.M32 + l.M30 * r.M33,
			/* l.M01, l.M11, l.M21, l.M31 */    l.M01 * r.M00 + l.M11 * r.M01 + l.M21 * r.M02 + l.M31 * r.M03, l.M01 * r.M10 + l.M11 * r.M11 + l.M21 * r.M12 + l.M31 * r.M13, l.M01 * r.M20 + l.M11 * r.M21 + l.M21 * r.M22 + l.M31 * r.M23, l.M01 * r.M30 + l.M11 * r.M31 + l.M21 * r.M32 + l.M31 * r.M33,
			/* l.M02, l.M12, l.M22, l.M32 */    l.M02 * r.M00 + l.M12 * r.M01 + l.M22 * r.M02 + l.M32 * r.M03, l.M02 * r.M10 + l.M12 * r.M11 + l.M22 * r.M12 + l.M32 * r.M13, l.M02 * r.M20 + l.M12 * r.M21 + l.M22 * r.M22 + l.M32 * r.M23, l.M02 * r.M30 + l.M12 * r.M31 + l.M22 * r.M32 + l.M32 * r.M33,
			/* l.M03, l.M13, l.M23, l.M33 */    l.M03 * r.M00 + l.M13 * r.M01 + l.M23 * r.M02 + l.M33 * r.M03, l.M03 * r.M10 + l.M13 * r.M11 + l.M23 * r.M12 + l.M33 * r.M13, l.M03 * r.M20 + l.M13 * r.M21 + l.M23 * r.M22 + l.M33 * r.M23, l.M03 * r.M30 + l.M13 * r.M31 + l.M23 * r.M32 + l.M33 * r.M33
			);
		}

		public static Vector4 operator * ( Matrix4x4 l, Vector4 r ) {
			return new Vector4(
				l.M00 * r.X + l.M10 * r.Y + l.M20 * r.Z + l.M30 * r.W,
				l.M01 * r.X + l.M11 * r.Y + l.M21 * r.Z + l.M31 * r.W,
				l.M02 * r.X + l.M12 * r.Y + l.M22 * r.Z + l.M32 * r.W,
				l.M03 * r.X + l.M13 * r.Y + l.M23 * r.Z + l.M33 * r.W
			);
		}

		public static Matrix4x4 CreatePerspectiveProjection ( float xSlope, float ySlope, float nearPlane, float farPlane ) {
			var fon = farPlane / nearPlane;
			var a = ( fon + 1 ) / ( fon - 1 );
			var b = farPlane * ( 1 - a );
			return new Matrix4x4(
				1 / xSlope, 0, 0, 0,
				0, 1 / ySlope, 0, 0,
				0, 0, a, b,
				0, 0, 1, 0
			);
		}

		public override string ToString () {
			return $"({M00}; {M10}; {M20}; {M30})\r\n({M01}; {M11}; {M21}; {M31})\r\n({M02}; {M12}; {M22}; {M32})\r\n({M03}; {M13}; {M23}; {M33})";
		}

		public override bool Equals ( object? obj ) {
			return obj is Matrix4x4 x && Equals( x );
		}

		public bool Equals ( Matrix4x4 other ) {
			return M00 == other.M00 &&
					 M10 == other.M10 &&
					 M20 == other.M20 &&
					 M30 == other.M30 &&
					 M01 == other.M01 &&
					 M11 == other.M11 &&
					 M21 == other.M21 &&
					 M31 == other.M31 &&
					 M02 == other.M02 &&
					 M12 == other.M12 &&
					 M22 == other.M22 &&
					 M32 == other.M32 &&
					 M03 == other.M03 &&
					 M13 == other.M13 &&
					 M23 == other.M23 &&
					 M33 == other.M33;
		}

		public override int GetHashCode () {
			HashCode hash = new HashCode();
			hash.Add( M00 );
			hash.Add( M10 );
			hash.Add( M20 );
			hash.Add( M30 );
			hash.Add( M01 );
			hash.Add( M11 );
			hash.Add( M21 );
			hash.Add( M31 );
			hash.Add( M02 );
			hash.Add( M12 );
			hash.Add( M22 );
			hash.Add( M32 );
			hash.Add( M03 );
			hash.Add( M13 );
			hash.Add( M23 );
			hash.Add( M33 );
			return hash.ToHashCode();
		}

		public static implicit operator Matrix4 ( Matrix4x4 m ) {
			return new Matrix4(
				m.M00, m.M10, m.M20, m.M30,
				m.M01, m.M11, m.M21, m.M31,
				m.M02, m.M12, m.M22, m.M32,
				m.M03, m.M13, m.M23, m.M33
			);
		}

		public static explicit operator Matrix4x4 ( Matrix4 m ) {
			return new Matrix4x4(
				m.M11, m.M21, m.M31, m.M41,
				m.M12, m.M22, m.M32, m.M42,
				m.M13, m.M23, m.M33, m.M43,
				m.M14, m.M24, m.M34, m.M44
			);
		}

		public Matrix4x4 Transposed
			=> new Matrix4x4(
				M00, M01, M02, M03,
				M10, M11, M12, M13,
				M20, M21, M22, M23,
				M30, M31, M32, M33
			);

		public static implicit operator Matrix4x4 ( HmdMatrix44_t matrix ) {
			return new Matrix4x4(
				matrix.m0, matrix.m1, matrix.m2, matrix.m3,
				matrix.m4, matrix.m5, matrix.m6, matrix.m7,
				matrix.m8, matrix.m9, matrix.m10, matrix.m11,
				matrix.m12, matrix.m13, matrix.m14, matrix.m15
			);
		}

		public static implicit operator Matrix4x4 ( HmdMatrix34_t matrix ) {
			return new Matrix4x4(
				matrix.m0, matrix.m1, matrix.m2, matrix.m3,
				matrix.m4, matrix.m5, matrix.m6, matrix.m7,
				matrix.m8, matrix.m9, -matrix.m10, matrix.m11,
				0, 0, 0, 1
			);
		}

		public static bool operator == ( Matrix4x4 l, Matrix4x4 r ) {
			return l.Row0 == r.Row0
				&& l.Row1 == r.Row1
				&& l.Row2 == r.Row2
				&& l.Row3 == r.Row3;
		}

		public static bool operator != ( Matrix4x4 l, Matrix4x4 r ) {
			return l.Row0 != r.Row0
				|| l.Row1 != r.Row1
				|| l.Row2 != r.Row2
				|| l.Row3 != r.Row3;
		}
	}
}
