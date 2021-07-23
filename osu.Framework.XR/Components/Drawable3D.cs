using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Projection;
using osuTK;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace osu.Framework.XR.Components {
	/// <summary>
	/// An <see cref="Drawable3D"/> is the 3D counterpart of a <see cref="Drawable"/>.
	/// </summary>
	public class Drawable3D : CompositeDrawable { // has to be a "Drawable" because it gives us cool stuff. It should eventually become a `(Composite)Component` when that is implemented
		public override bool RemoveCompletedTransforms => true;
		public Drawable3D () {
			Transform = new Transform( transformKey );
			RelativeSizeAxes = Axes.Both;
			AlwaysPresent = true;
		}

		internal CompositeDrawable3D? parent;
		new public CompositeDrawable3D? Parent {
			get => parent;
			set {
				if ( parent == value ) return;

				if ( parent is not null ) {
					parent.children.Remove( this );
					parent.RemoveDrawable( this );

					parent.onChildRemoved( this );
					foreach ( var i in GetAllChildrenInHiererchy() ) parent.onChildRemovedFromHierarchy( i.parent!, i );
				}
				parent = value;
				if ( parent is not null ) {
					parent.children.Add( this );
					parent.AddDrawable( this ); // this is here so they actually exist in the framework heirerchy

					parent.onChildAdded( this );
					foreach ( var i in GetAllChildrenInHiererchy() ) parent.onChildAddedToHierarchy( i.parent!, i );
				}
				Transform.SetParent( parent?.Transform, transformKey );
			}
		}

		public IEnumerable<Drawable3D> GetAllChildrenInHiererchy () {
			List<Drawable3D> all = new() { this };
			for ( int i = 0; i < all.Count; i++ ) {
				if ( all[ i ] is CompositeDrawable3D current ) {
					for ( int k = 0; k < current.children.Count; k++ ) {
						yield return current.children[ k ];
						all.Add( current.children[ k ] );
					}
				}
			}
		}

		/// <summary>
		/// The topmost <see cref="Container3D"/> in the hierarchy. This operation performs upwards tree traveral and might be expensive.
		/// </summary>
		public Container3D Root => parent?.Root ?? ( this as Container3D ?? throw new Exception( $"The topmost element in the scene is not a {nameof(Container3D)}" ) );
		public T? FindObject<T> () where T : Drawable3D {
			T? find ( CompositeDrawable3D node ) {
				if ( node is T tnode ) return tnode;
				foreach ( var i in node.children.OfType<CompositeDrawable3D>() ) {
					var res = find( i );
					if ( res is not null ) return res;
				}
				return null;
			}

			return find( Root );
		}

		public virtual void BeforeDraw ( DrawNode3D.DrawSettings settings ) { }

		protected readonly object transformKey = new { };
		public readonly Transform Transform;
		new public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
		public Vector3 GlobalPosition {
			get => Transform.GlobalPosition;
			set => Transform.GlobalPosition = value;
		}
		new public float X { get => Transform.X; set => Transform.X = value; }
		new public float Y { get => Transform.Y; set => Transform.Y = value; }
		public float Z { get => Transform.Z; set => Transform.Z = value; }

		new public Vector3 Scale { get => Transform.Scale; set => Transform.Scale = value; }
		public float ScaleX { get => Transform.ScaleX; set => Transform.ScaleX = value; }
		public float ScaleY { get => Transform.ScaleY; set => Transform.ScaleY = value; }
		public float ScaleZ { get => Transform.ScaleZ; set => Transform.ScaleZ = value; }

		public Vector3 Offset {
			get => Transform.Offset;
			set {
				AutoOffsetAxes = Axes3D.None;
				Transform.Offset = value;
			}
		}
		public float OffsetX {
			get => Transform.OffsetX;
			set {
				AutoOffsetAxes &= ~Axes3D.X;
				Transform.OffsetX = value;
			}
		}
		public float OffsetY {
			get => Transform.OffsetY;
			set {
				AutoOffsetAxes &= ~Axes3D.Y;
				Transform.OffsetY = value;
			}
		}
		public float OffsetZ {
			get => Transform.OffsetZ;
			set {
				AutoOffsetAxes &= ~Axes3D.Z;
				Transform.OffsetZ = value;
			}
		}

		new public Quaternion Rotation { get => Transform.Rotation; set => Transform.Rotation = value; }
		public Vector3 EulerRotation { get => Transform.EulerRotation; set => Transform.EulerRotation = value; }
		public float EulerRotX { get => Transform.EulerRotX; set => Transform.EulerRotX = value; }
		public float EulerRotY { get => Transform.EulerRotY; set => Transform.EulerRotY = value; }
		public float EulerRotZ { get => Transform.EulerRotZ; set => Transform.EulerRotZ = value; }
		public Quaternion GlobalRotation {
			get => Transform.GlobalRotation;
			set => Transform.GlobalRotation = value;
		}

		public Vector3 Forward => Transform.Forward;
		public Vector3 Backward => Transform.Backward;
		public Vector3 Left => Transform.Left;
		public Vector3 Right => Transform.Right;
		public Vector3 Up => Transform.Up;
		public Vector3 Down => Transform.Down;

		public Vector3 GlobalForward => Transform.GlobalForward;
		public Vector3 GlobalBackward => Transform.GlobalBackward;
		public Vector3 GlobalLeft => Transform.GlobalLeft;
		public Vector3 GlobalRight => Transform.GlobalRight;
		public Vector3 GlobalUp => Transform.GlobalUp;
		public Vector3 GlobalDown => Transform.GlobalDown;

		new public virtual Vector3 Size { get; set; }
		public virtual Vector3 RequiredParentSizeToFit => Size;

		public Axes3D AutoOffsetAxes = Axes3D.None;
		new public Axes3D BypassAutoSizeAxes = Axes3D.None;
		private Vector3 autoOffsetAnchor;
		public Vector3 AutoOffsetAnchor {
			get => autoOffsetAnchor;
			set {
				AutoOffsetAxes = Axes3D.All;
				autoOffsetAnchor = value;
			}
		}
		public float AutoOffsetAnchorX {
			get => autoOffsetAnchor.X;
			set {
				AutoOffsetAxes |= Axes3D.X;
				autoOffsetAnchor.X = value;
			}
		}
		public float AutoOffsetAnchorY {
			get => autoOffsetAnchor.Y;
			set {
				AutoOffsetAxes |= Axes3D.Y;
				autoOffsetAnchor.Y = value;
			}
		}
		public float AutoOffsetAnchorZ {
			get => autoOffsetAnchor.Z;
			set {
				AutoOffsetAxes |= Axes3D.Z;
				autoOffsetAnchor.Z = value;
			}
		}
		private Vector3 autoOffsetOrigin;
		public Vector3 AutoOffsetOrigin {
			get => autoOffsetOrigin;
			set {
				AutoOffsetAxes = Axes3D.All;
				autoOffsetOrigin = value;
			}
		}
		public float AutoOffsetOriginX {
			get => autoOffsetOrigin.X;
			set {
				AutoOffsetAxes |= Axes3D.X;
				autoOffsetOrigin.X = value;
			}
		}
		public float AutoOffsetOriginY {
			get => autoOffsetOrigin.Y;
			set {
				AutoOffsetAxes |= Axes3D.Y;
				autoOffsetOrigin.Y = value;
			}
		}
		public float AutoOffsetOriginZ {
			get => autoOffsetOrigin.Z;
			set {
				AutoOffsetAxes |= Axes3D.Z;
				autoOffsetOrigin.Z = value;
			}
		}

		/// <summary>
		/// The centre of the object in local coordinates.
		/// </summary>
		public virtual Vector3 Centre => Vector3.Zero;
		protected override void Update () {
			base.Update();

			if ( AutoOffsetAxes.HasFlag( Axes3D.X ) ) {
				var parentSize = parent is null ? 0 : parent.ChildSize.X;
				var ownSize = Size.X;
				Transform.OffsetX = autoOffsetAnchor.X * parentSize - autoOffsetOrigin.X * ownSize - Centre.X;
			}
			if ( AutoOffsetAxes.HasFlag( Axes3D.Y ) ) {
				var parentSize = parent is null ? 0 : parent.ChildSize.Y;
				var ownSize = Size.Y;
				Transform.OffsetY = autoOffsetAnchor.Y * parentSize - autoOffsetOrigin.Y * ownSize - Centre.Y;
			}
			if ( AutoOffsetAxes.HasFlag( Axes3D.Z ) ) {
				var parentSize = parent is null ? 0 : parent.ChildSize.Z;
				var ownSize = Size.Z;
				Transform.OffsetZ = autoOffsetAnchor.Z * parentSize - autoOffsetOrigin.Z * ownSize - Centre.Z;
			}
		}

		public virtual void Destroy () {
			Parent = null;
			Dispose();
		}

		public void AddDrawable ( Drawable drawable ) {
			base.AddInternal( drawable );
		}
		public void RemoveDrawable ( Drawable drawable ) {
			base.RemoveInternal( drawable );
		}

		public bool ShouldBeDepthSorted { get; init; } = false;
		private DrawNode3D? drawNode;
		public DrawNode3D? DrawNode => drawNode ??= CreateDrawNode();
		new protected virtual DrawNode3D? CreateDrawNode () => null;

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
			drawNode?.Dispose();
		}
		public abstract class DrawNode3D : IDisposable {
			protected Drawable3D Source;
			protected Transform Transform => Source.Transform;
			public DrawNode3D ( Drawable3D source ) {
				Source = source;
			}

			public abstract void Draw ( DrawSettings settings );

			public virtual void Dispose () { }

			public record DrawSettings { // TODO most of these should be in a global uniform block
				public Matrix4 WorldToCamera { get; init; }
				public Matrix4 CameraToClip { get; init; }
				public Vector3 GlobalCameraPos { get; init; }
				public Quaternion GlobalCameraRot { get; init; }
				[MaybeNull, NotNull]
				public Camera Camera { get; init; }
			}
		}

		public abstract class XrObjectDrawNode<T> : DrawNode3D where T : Drawable3D {
			new protected T Source => (T)base.Source;
			public XrObjectDrawNode ( T source ) : base( source ) { }
		}

		public static implicit operator Transform? ( Drawable3D? xro )
			=> xro?.Transform;
	}
	[Flags]
	public enum Axes3D {
		None = 0,

		X = 1,
		Y = 2,
		Z = 4,

		XY = X | Y,
		XZ = X | Z,
		YZ = Y | Z,
		All = X | Y | Z
	}
}