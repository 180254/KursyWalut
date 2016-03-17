using System;
using System.Collections.Generic;

namespace KursyWalut.Provider
{
    internal class ObservationDispose<T> : IDisposable
    {
        private readonly IList<IObserver<T>> _observers;
        private readonly IObserver<T> _observer;

        public ObservationDispose(IList<IObserver<T>> observers, IObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            _observers.Remove(_observer);
        }
    }
}