using System;

namespace KursyWalut.Cache
{
    internal interface ICache
    {
        void Set<TR>(string key, TR value);

        /// <exception cref="T:SystemInvalidCastException">Value for <paramref name="key" /> is not of type TR.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Value for <paramref name="key" /> not found.</exception>
        TR Get<TR>(string key);

        /// <exception cref="T:SystemInvalidCastException">Value for <paramref name="key" /> is not of type TR.</exception>
        TR GetOrSet<TR>(string key, TR value);

        /// <exception cref="T:SystemInvalidCastException">Value for <paramref name="key" /> is not of type TR.</exception>
        TR GetOrCompute<TR>(string key, Func<TR> valueFunc);
    }
}