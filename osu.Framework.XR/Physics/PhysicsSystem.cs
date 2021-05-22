using osu.Framework.XR.Components;
using osuTK;
using System;
using System.Collections.Generic;
using static osu.Framework.XR.Physics.Raycast;

namespace osu.Framework.XR.Physics {
	// TODO add physics layers (64b flag)
	public class PhysicsSystem : IDisposable {
		private List<(IHasCollider collider, Model model)> modelColliders = new();
		private List<(IHasCollider collider, Drawable3D model)> drawableColliders = new();
		private CompositeDrawable3D? root;
		public CompositeDrawable3D? Root {
			get => root;
			set {
				if ( root == value ) return;

				if ( root is not null ) {
					root.ChildAddedToHierarchy -= addXrObject;
					root.ChildRemovedFromHierarchy -= removeXrObject;
				}
				clearColliders();
				root = value;
				root?.BindHierarchyChange( addXrObject, removeXrObject, true );
			}
		}
		void clearColliders () {
			modelColliders.Clear();
			drawableColliders.Clear();
		}

		private void addXrObject ( Drawable3D parent, Drawable3D child ) {
			if ( child is IHasCollider collider ) {
				if ( child is Model model ) {
					modelColliders.Add( (collider, model) );
				}
				else {
					drawableColliders.Add( (collider, child) );
				}
			}
		}
		private void removeXrObject ( Drawable3D parent, Drawable3D child ) {
			if ( child is IHasCollider collider ) {
				if ( child is Model model ) {
					modelColliders.Remove( (collider, model) );
				}
				else {
					drawableColliders.Remove( (collider, child) );
				}
			}
		}

		/// <summary>
		/// Intersect a ray and a the closest collider.
		/// </summary>
		public bool TryHit ( Vector3 origin, Vector3 direction, out RaycastHit hit, bool includeBehind = false, PhysicsLayer layer = PhysicsLayer.All ) {
			RaycastHit? closest = null;
			IHasCollider? closestCollider = null;
			direction.Normalize();

			for ( int i = 0; i < modelColliders.Count; i++ ) {
				var (collider, model) = modelColliders[ i ];
				if ( (collider.PhysicsLayer & layer) != PhysicsLayer.None && collider.IsColliderEnabled && Raycast.TryHitPrenormalized( origin, direction, model, out hit, includeBehind ) ) {
					if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
						closest = hit;
						closestCollider = collider;
					}
				}
			}

			for ( int i = 0; i < drawableColliders.Count; i++ ) {
				var (collider, drawable) = drawableColliders[ i ];
				if ( ( collider.PhysicsLayer & layer ) != PhysicsLayer.None && collider.IsColliderEnabled && Raycast.TryHitPrenormalized( origin, direction, collider.Mesh, drawable.Transform, out hit, includeBehind ) ) {
					if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
						closest = hit;
						closestCollider = collider;
					}
				}
			}

			if ( closest is null ) {
				hit = default;
				return false;
			}
			else {
				hit = closest.Value;
				hit = new RaycastHit(
					hit.Point,
					hit.Origin,
					hit.Normal,
					hit.Direction,
					hit.Distance,
					hit.TrisIndex,
					closestCollider
				);
				return true;
			}
		}

		/// <summary>
		/// Intersect a sphere and the closest collider.
		/// </summary>
		public bool TryHit ( Vector3 origin, double radius, out SphereHit hit, PhysicsLayer layer = PhysicsLayer.All ) {
			SphereHit? closest = null;
			IHasCollider? closestCollider = null;

			for ( int i = 0; i < modelColliders.Count; i++ ) {
				var (collider, model) = modelColliders[ i ];
				if ( ( collider.PhysicsLayer & layer ) != PhysicsLayer.None && collider.IsColliderEnabled && Sphere.TryHit( origin, radius, model, out hit ) ) {
					if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
						closest = hit;
						closestCollider = collider;
					}
				}
			}

			for ( int i = 0; i < drawableColliders.Count; i++ ) {
				var (collider, drawable) = drawableColliders[ i ];
				if ( ( collider.PhysicsLayer & layer ) != PhysicsLayer.None && collider.IsColliderEnabled && Sphere.TryHit( origin, radius, collider.Mesh, drawable.Transform, out hit ) ) {
					if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
						closest = hit;
						closestCollider = collider;
					}
				}
			}

			if ( closest is null ) {
				hit = default;
				return false;
			}
			else {
				hit = closest.Value;
				hit = new SphereHit(
					hit.Distance,
					hit.Origin,
					hit.Radius,
					hit.Point,
					hit.TrisIndex,
					closestCollider
				);
				return true;
			}
		}

		public void Dispose () {
			Root = null;
		}
	}
}
