using osu.Framework.XR.Collections;
using osu.Framework.XR.Graphics;

namespace osu.Framework.XR.Physics;

public class PhysicsSystem {
	HashList<IHasCollider> colliders = new();
	Dictionary<CompositeDrawable3D, SubtreeModifiedSubscription> subscriptions = new();

	public void AddSubtree ( CompositeDrawable3D root ) {
		if ( subscriptions.ContainsKey( root ) )
			return;

		subscriptions.Add( root, root.SubscribeSubtreeModified(
			added: ( child, parent, _ ) => {
				if ( child is IHasCollider collider )
					colliders.Add( collider );
			},
			removed: ( child, parent, _ ) => {
				if ( child is IHasCollider collider )
					colliders.Remove( collider );
			}
		) );
	}

	public void RemoveSubtree ( CompositeDrawable3D root ) {
		if ( !subscriptions.Remove( root, out var subscription ) )
			return;

		root.UnsubscribeSubtreeModified( subscription, invokeRemoved: true );
	}

	public void Clear () {
		while ( subscriptions.Any() )
			RemoveSubtree( subscriptions.Keys.First() );
	}

	/// <summary>
	/// Intersect a ray and a the closest collider.
	/// </summary>
	public bool TryHitRay ( Vector3 origin, Vector3 direction, out RaycastHit hit, bool includeBehind = false, ulong layers = ulong.MaxValue ) {
		RaycastHit? closest = null;
		direction.Normalize();

		foreach ( var collider in colliders.AsSpan() ) {
			if ( !collider.IsColliderEnabled || ( collider.PhysicsLayer & layers ) == 0 )
				continue;

			if ( Raycast.TryHitPrenormalized( origin, direction, collider, out hit, includeBehind ) ) {
				if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
					closest = hit;
				}
			}
		}

		if ( closest is null ) {
			hit = default;
			return false;
		}
		else {
			hit = closest.Value;
			return true;
		}
	}

	/// <summary>
	/// Intersect a sphere and the closest collider.
	/// </summary>
	public bool TryHitSphere ( Vector3 origin, double radius, out SphereHit hit, ulong layers = ulong.MaxValue ) {
		SphereHit? closest = null;

		foreach ( var collider in colliders.AsSpan() ) {
			if ( !collider.IsColliderEnabled || ( collider.PhysicsLayer & layers ) == 0 )
				continue;

			if ( Sphere.TryHit( origin, radius, collider, out hit ) ) {
				if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
					closest = hit;
				}
			}
		}

		if ( closest is null ) {
			hit = default;
			return false;
		}
		else {
			hit = closest.Value;
			return true;
		}
	}
}