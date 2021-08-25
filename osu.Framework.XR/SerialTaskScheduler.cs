using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace osu.Framework.XR {
	/// <summary>
	/// A schedular that performs tasks sequentially.
	/// </summary>
	public class SerialTaskScheduler {
		ConcurrentQueue<Action> taskQueue = new();
		Task? currentTask;

		public event Action? TaskAdded;

		public event Action? TaskStarted;
		public event Action? TaskFinished;

		public event Action? TaskSequenceStarted;
		public event Action? TaskSequenceFinished;

		public void Schedule ( Action task ) {
			taskQueue.Enqueue( task );
			TaskAdded?.Invoke();

			runNext( isSequential: false );
		}

		private void runNext ( bool isSequential ) {
			if ( currentTask is null && taskQueue.TryDequeue( out var task ) ) {
				currentTask = Task.Run( () => {
					if ( !isSequential ) TaskSequenceStarted?.Invoke();
					TaskStarted?.Invoke();
					task();
					TaskFinished?.Invoke();
					currentTask = null;
					runNext( isSequential: true );
				} );
			}
			else if ( isSequential ) {
				TaskSequenceFinished?.Invoke();
			}
		}
	}
}
