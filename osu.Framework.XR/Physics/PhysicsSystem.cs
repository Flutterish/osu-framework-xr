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
		direction.Normalize();

		bool hasResult = false;
		RaycastHit hitA = new();
		RaycastHit hitB = new();
		ref RaycastHit closest = ref hitA;
		ref RaycastHit swap = ref hitB;

		foreach ( var collider in colliders.AsSpan() ) {
			if ( !collider.IsColliderEnabled || ( collider.PhysicsLayer & layers ) == 0 )
				continue;

			if ( Raycast.TryHitPrenormalized( origin, direction, collider, ref swap, includeBehind ) ) {
				if ( !hasResult || Math.Abs( closest.Distance ) > Math.Abs( swap.Distance ) ) {
					ref RaycastHit temp = ref closest;
					closest = swap;
					swap = temp;
					hasResult = true;
				}
			}
		}

		if ( hasResult ) {
			hit = closest;
			return true;
		}
		else {
			hit = default;
			return false;
		}
	}

	/// <summary>
	/// Intersect a sphere and the closest collider.
	/// </summary>
	public bool TryHitSphere ( Vector3 origin, double radius, out SphereHit hit, ulong layers = ulong.MaxValue ) {
		bool hasResult = false;
		SphereHit hitA = new();
		SphereHit hitB = new();
		ref SphereHit closest = ref hitA;
		ref SphereHit swap = ref hitB;

		foreach ( var collider in colliders.AsSpan() ) {
			if ( !collider.IsColliderEnabled || ( collider.PhysicsLayer & layers ) == 0 )
				continue;

			if ( Sphere.TryHit( origin, radius, collider, ref swap ) ) {
				if ( !hasResult || Math.Abs( closest.Distance ) > Math.Abs( swap.Distance ) ) {
					ref SphereHit temp = ref closest;
					closest = swap;
					swap = temp;
					hasResult = true;
				}
			}
		}

		if ( hasResult ) {
			hit = closest;
			return true;
		}
		else {
			hit = default;
			return false;
		}
	}
}