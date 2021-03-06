using osu.Framework.Bindables;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Primitives;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osuTK;
using osuTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Projection {
	public class Camera : Drawable3D {
		public Camera () {
			Fov = new Vector2( MathF.Atan( 16f / 9 ) * 2, MathF.Atan( 1 ) * 2 );
			FovBindable.BindValueChanged( v => {
				XSlope = MathF.Tan( v.NewValue.X / 2 );
				YSlope = MathF.Tan( v.NewValue.Y / 2 );
				AspectRatio = XSlope / YSlope;
				CameraClipMatrix = Matrix4x4.CreatePerspectiveProjection( XSlope, YSlope, 0.01f, 1000 );
			}, true );
		}

		HashSet<Drawable3D> allDrawables = new();

		List<Drawable3D> finalRenderTargets = new();
		List<Drawable3D> depthSortedRenderTargets = new();
		List<Drawable3D> depthTestedRenderTargets = new();
		List<Drawable3D> renderTargets = new();
		private bool shouldBeDepthTested ( Drawable3D target ) {
			return target is not IBehindEverything;
		}
		private bool shouldBeRenderedLast ( Drawable3D target ) {
			return target is IRenderedLast;
		}
		private bool shouldBeDepthSorted ( Drawable3D target ) {
			return target.ShouldBeDepthSorted;
		}

		private void addRenderTarget ( Drawable3D parent, Drawable3D child ) {
			if ( allDrawables.Contains( child ) ) {
				throw new InvalidOperationException( "Tried to add a render target that was already registered" );
			}
			allDrawables.Add( child );

			if ( shouldBeRenderedLast( child ) )
				lock ( finalRenderTargets ) { finalRenderTargets.Add( child ); }
			else if ( shouldBeDepthSorted( child ) )
				lock ( depthSortedRenderTargets ) { depthSortedRenderTargets.Add( child ); }
			else if ( shouldBeDepthTested( child ) )
				lock ( depthTestedRenderTargets ) { depthTestedRenderTargets.Add( child ); }
			else
				lock ( renderTargets ) { renderTargets.Add( child ); }
		}
		private void removeRenderTarget ( Drawable3D parent, Drawable3D child ) {
			if ( !allDrawables.Contains( child ) ) {
				throw new InvalidOperationException( "Tried to remove a render target that was not registered" );
			}
			allDrawables.Remove( child );

			if ( shouldBeRenderedLast( child ) )
				lock ( finalRenderTargets ) { finalRenderTargets.Remove( child ); }
			else if ( shouldBeDepthSorted( child ) )
				lock ( depthSortedRenderTargets ) { depthSortedRenderTargets.Remove( child ); }
			else if ( shouldBeDepthTested( child ) )
				lock ( depthTestedRenderTargets ) { depthTestedRenderTargets.Remove( child ); }
			else
				lock ( renderTargets ) { renderTargets.Remove( child ); }
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Root.BindHierarchyChange( addRenderTarget, removeRenderTarget, true );
		}

		/// <summary>
		/// Field of view in radians.
		/// </summary>
		public Vector2 Fov {
			get => FovBindable.Value;
			set {
				FovBindable.Value = value;
			}
		}
		public readonly Bindable<Vector2> FovBindable = new();
		public float AspectRatio { get; private set; }
		public float XSlope { get; private set; }
		public float YSlope { get; private set; }
		public Matrix4x4 WorldCameraMatrix => Transform.InverseMatrix;
		public Matrix4x4 CameraClipMatrix { get; private set; }
		public Matrix4x4 WorldClipMatrix => CameraClipMatrix * WorldCameraMatrix;

		/// <summary>
		/// Projects a given point to <-1;1><-1;1>. Returns false if the point is behind the camera.
		/// </summary>
		public bool Project ( Vector3 pos, out Vector2 proj ) {
			var p = WorldClipMatrix * new Vector4( pos, 1 );
			proj = p.Xy / p.W;

			return p.Z > 0;
		}

		/// <summary>
		/// Projects a given point to <0;width><0;height>. Returns false if the point is behind the camera.
		/// </summary>
		public bool Project ( Vector3 pos, float width, float height, out Vector2 proj ) {
			var scale = createScale( width, height );

			var p = ( Matrix4x4.CreateScale( scale.X, scale.Y ) * WorldClipMatrix ) * new Vector4( pos, 1 );
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
			var scale = createScale( width, height );

			return (
				Forward
				+ Right * ( pos.X / width - 0.5f ) * 2 * XSlope / scale.X
				+ Down * ( pos.Y / height - 0.5f ) * 2 * YSlope / scale.Y
			).Normalized();
		}

		/// <summary>
		/// Computes a normal vector pointing at a given screenspace position.
		/// </summary>
		public Vector3 GlobalDirectionOf ( Vector2 pos, float width, float height ) {
			var scale = createScale( width, height );

			return (
				GlobalForward
				+ GlobalRight * ( pos.X / width - 0.5f ) * 2 * XSlope / scale.X
				+ GlobalDown * ( pos.Y / height - 0.5f ) * 2 * YSlope / scale.Y
			).Normalized();
		}

		/// <summary>
		/// Creates a scale vector which fits the renderspace into the target rect while padding excess space.
		/// </summary>
		private Vector2 createScale ( float targetWidth, float targetHeight ) {
			var ratio = targetWidth / targetHeight;
			if ( ratio > AspectRatio ) {
				return new Vector2( AspectRatio / ratio, 1 );
			}
			else {
				return new Vector2( 1, ratio / AspectRatio );
			}
		}

		public void Render ( DepthFrameBuffer depthBuffer ) {
			var scale = createScale( depthBuffer.Size.X, depthBuffer.Size.Y );

			var settings = new DrawNode3D.DrawSettings {
				Camera = this,
				CameraToClip = CameraClipMatrix.Transposed,
				WorldToCamera = (Matrix4x4.CreateScale( scale.X, scale.Y ) * WorldCameraMatrix).Transposed,
				GlobalCameraPos = GlobalPosition,
				GlobalCameraRot = GlobalRotation
			};

			Render( depthBuffer, settings );
		}

		public void Render ( DepthFrameBuffer depthBuffer, DrawNode3D.DrawSettings settings ) {
			settings = settings with { Camera = this };

			GLWrapper.PushViewport( new RectangleI( 0, 0, (int)depthBuffer.Size.X, (int)depthBuffer.Size.Y ) );
			GLWrapper.PushScissor( new RectangleI( 0, 0, (int)depthBuffer.Size.X, (int)depthBuffer.Size.Y ) );
			GLWrapper.PushScissorOffset( Vector2I.Zero );
			depthBuffer.Bind();
			GLWrapper.PushDepthInfo( new DepthInfo( false, false, osuTK.Graphics.ES30.DepthFunction.Less ) );
			GL.Clear( ClearBufferMask.ColorBufferBit );
			lock ( renderTargets ) {
				foreach ( var i in renderTargets ) {
					i.DrawNode?.Draw( settings );
				}
			}

			GLWrapper.PushDepthInfo( new DepthInfo( true, true, osuTK.Graphics.ES30.DepthFunction.Less ) );
			GL.Clear( ClearBufferMask.DepthBufferBit );
			lock ( depthTestedRenderTargets ) {
				foreach ( var i in depthTestedRenderTargets ) {
					i.DrawNode?.Draw( settings );
				}
			}
			lock ( depthSortedRenderTargets ) {
				foreach ( var i in depthSortedRenderTargets.OrderByDescending( d => {
					var p = settings.WorldToCamera * d.Transform.Matrix * new Vector4( d.Centre, 1 );
					return p.Z / p.W;
				} ) ) {
					i.DrawNode?.Draw( settings );
				}
			}
			lock ( finalRenderTargets ) {
				foreach ( var i in finalRenderTargets ) {
					i.DrawNode?.Draw( settings );
				}
			}

			GLWrapper.PopDepthInfo();
			GLWrapper.PopDepthInfo();
			depthBuffer.Unbind();
			GLWrapper.PopScissorOffset();
			GLWrapper.PopScissor();
			GLWrapper.PopViewport();
		}

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
			var root = Root;
			root.ChildAddedToHierarchy -= addRenderTarget;
			root.ChildRemovedFromHierarchy -= removeRenderTarget;
		}
	}
}
