namespace osu.Framework.XR;

public class TripleBuffer<T> where T : class {
	private readonly ObjectUsage<T>[] buffers = new ObjectUsage<T>[3];

	private int lastCompletedWriteIndex;

	private int? activeReadIndex;

	private const int buffer_count = 3;

	public TripleBuffer () {
		for ( int i = 0; i < buffer_count; i++ )
			buffers[i] = new ObjectUsage<T>( i, finishUsage );
	}

	public ObjectUsage<T> GetForWrite () {
		ObjectUsage<T> buffer;

		lock ( buffers ) {
			buffer = getNextWriteBuffer();

			buffer.Usage = UsageType.Write;
		}

		return buffer;
	}

	public ObjectUsage<T> GetForRead () {
		lock ( buffers ) {
			var buffer = buffers[lastCompletedWriteIndex];
			buffer.Usage = UsageType.Read;

			activeReadIndex = buffer.Index;
			return buffer;
		}
	}

	private ObjectUsage<T> getNextWriteBuffer () {
		for ( int i = 0; i < buffer_count - 1; i++ ) {
			if ( i == activeReadIndex ) continue;
			if ( i == lastCompletedWriteIndex ) continue;

			return buffers[i];
		}

		return buffers[buffer_count - 1];
	}

	private void finishUsage ( ObjectUsage<T> obj ) {
		lock ( buffers ) {
			switch ( obj.Usage ) {
				case UsageType.Read:
					activeReadIndex = null;
					break;

				case UsageType.Write:
					lastCompletedWriteIndex = obj.Index;
					break;
			}

			obj.Usage = UsageType.None;
		}
	}
}
