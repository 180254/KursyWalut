using System;

namespace KursyWalut.Cache
{
    internal interface ICache
    {
        /// Info: default_ is invoked(computed), it will be not stored.
        /// <exception cref="T:System.InvalidCastException">Value for <paramref name="key" /> is not of type T.</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        T Get<T>(string key, Func<T> default_);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        void Store<T>(string key, T value);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        bool Remove(string key);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        void Clear();
    }
}