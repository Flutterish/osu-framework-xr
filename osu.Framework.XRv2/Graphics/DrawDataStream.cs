using osu.Framework.Graphics;

namespace osu.Framework.XR.Graphics;

/// <summary>
/// A component which can quickly upload data to the draw thread instead of creating a custom draw node
/// </summary>
public class DrawDataStream<T> : Drawable3D {
	T data;
	bool hasNewData;
	public T Data {
		get => data;
		set {
			data = value;
			hasNewData = true;
			Invalidate( Invalidation.DrawNode );
		}
	}

	Action<T> uploadAction;
	public DrawDataStream ( T initialValue, Action<T> uploadAction ) {
		this.uploadAction = uploadAction;
		data = initialValue;
	}

	protected override DrawNode3D? CreateDrawNode3D ()
		=> new Node( this );

	class Node : DrawNode3D {
		new DrawDataStream<T> Source => (DrawDataStream<T>)base.Source;
		T data = default!;
		bool newData;

		public Node ( DrawDataStream<T> source ) : base( source ) { }

		protected override void UpdateState () {
			data = Source.data;
			newData = Source.hasNewData;
			Source.hasNewData = false;
		}

		public override void Draw ( object? ctx = null ) {
			if ( newData ) {
				Source.uploadAction( data );
				newData = false;
			}
		}
	}
}
