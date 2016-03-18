using System;

namespace KursyWalut.Cache
{
    internal interface ICache
    {
        /// <exception cref="T:System.InvalidCastException">Value for <paramref name="key" /> is not of type T.</exception>
        T Get<T>(string key, Func<T> default_);

        void Store<T>(string key, T value);

        bool Remove(string key);

        void Clear();
    }
}