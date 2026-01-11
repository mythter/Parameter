using System;

namespace Parameter.Observers
{
	public class SimpleObserver<T> : IObserver<T>
	{
		private readonly Action<T> _onNext;

		public SimpleObserver(Action<T> onNext)
		{
			_onNext = onNext;
		}

		public void OnCompleted() { }

		public void OnError(Exception error) { }

		public void OnNext(T value) => _onNext(value);
	}

}
