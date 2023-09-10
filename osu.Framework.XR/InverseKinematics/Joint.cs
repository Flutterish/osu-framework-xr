namespace osu.Framework.XR.InverseKinematics;

/// <summary>
/// A joint defined position, orientation and constraints relative to other joints.
/// Links define the structure, while the joint itself defines a constraint between itself and its parent.
/// </summary>
public class Joint {
	/// <summary>
	/// Position in global space.
	/// </summary>
	public Vector3 Position;
	/// <summary>
	/// Rotation in global space.
	/// </summary>
	public Quaternion Rotation = Quaternion.Identity;

	public readonly List<Link> Links = new();

	public Link Child { init => Links.Add( value ); }
	public IEnumerable<Link> Children { init => Links.AddRange( value ); }

	/// <summary>
	/// Applies constraints to the parent joint.
	/// </summary>
	public virtual void ConstrainParent ( Joint parent ) {

	}

	/// <summary>
	/// Applies constraints to this joint given a fixed parent joint.
	/// </summary>
	public virtual void ConstrainSelf ( Joint parent ) {
		
	}
}
