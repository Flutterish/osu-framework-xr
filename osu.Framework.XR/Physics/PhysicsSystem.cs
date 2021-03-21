﻿using osu.Framework.XR.Components;
using osuTK;
using System;
using System.Collections.Generic;
using static osu.Framework.XR.Physics.Raycast;

namespace osu.Framework.XR.Physics {
	public class PhysicsSystem : IDisposable {
		private List<IHasCollider> colliders = new();
		private CompositeDrawable3D root;
		public CompositeDrawable3D Root {
			get => root;
			set {
				if ( root == value ) return;

				if ( root is not null ) {
					root.ChildAddedToHierarchy -= addXrObject;
					root.ChildRemovedFromHierarchy -= removeXrObject;
				}
				colliders.Clear();
				root = value;
				root?.BindHierarchyChange( addXrObject, removeXrObject, true );
			}
		}

		private void addXrObject ( Drawable3D parent, Drawable3D child ) {
			if ( child is IHasCollider collider ) {
				colliders.Add( collider );
			}
		}
		private void removeXrObject ( Drawable3D parent, Drawable3D child ) {
			if ( child is IHasCollider collider ) {
				colliders.Remove( collider );
			}
		}

		/// <summary>
		/// Intersect a 3D line and a the closest collider.
		/// </summary>
		public bool TryHit ( Vector3 origin, Vector3 direction, out RaycastHit hit, bool includeBehind = false ) {
			RaycastHit? closest = null;
			IHasCollider closestCollider = null;
			direction.Normalize();

			for ( int i = 0; i < colliders.Count; i++ ) {
				var collider = colliders[ i ];
				if ( collider is Model model ) {
					if ( collider.IsColliderEnabled && Raycast.TryHit( origin, direction, collider as Model, out hit, includeBehind ) ) {
						if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
							closest = hit;
							closestCollider = collider;
						}
					}
				}
				else {
					if ( collider.IsColliderEnabled && Raycast.TryHit( origin, direction, collider.Mesh, ( collider as Drawable3D ).Transform, out hit, includeBehind ) ) {
						if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
							closest = hit;
							closestCollider = collider;
						}
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
		/// Intersect a shpere and a the closest collider.
		/// </summary>
		public bool TryHit ( Vector3 origin, double radius, out SphereHit hit ) {
			SphereHit? closest = null;
			IHasCollider closestCollider = null;

			for ( int i = 0; i < colliders.Count; i++ ) {
				var collider = colliders[ i ];
				if ( collider is Model model ) {
					if ( collider.IsColliderEnabled && Sphere.TryHit( origin, radius, model, out hit ) ) {
						if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
							closest = hit;
							closestCollider = collider;
						}
					}
				}
				else {
					if ( collider.IsColliderEnabled && Sphere.TryHit( origin, radius, collider.Mesh, ( collider as Drawable3D ).Transform, out hit ) ) {
						if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
							closest = hit;
							closestCollider = collider;
						}
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
