using System;

namespace KursyWalut.Cache
{
    internal interface ICache
    {
        void Set<TR>(string key, TR value);

        /// <exception cref="T:System.InvalidCastException">Value for <paramref name="key" /> is not of type TR.</exception>
        TR SetIfAbsent<TR>(string key, TR value);

        TR Compute<TR>(string key, Func<TR> valueFunc);

        /// <exception cref="T:System.InvalidCastException">Value for <paramref name="key" /> is not of type TR.</exception>
        TR ComputeIfAbsent<TR>(string key, Func<TR> valueFunc);

        /// <exception cref="T:System.InvalidCastException">Value for <paramref name="key" /> is not of type TR.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Value for <paramref name="key" /> not found.</exception>
        TR Get<TR>(string key);

        bool Remove(string key);
    }
}