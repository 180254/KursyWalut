using System;
using System.Collections.Generic;

namespace KursyWalut.Cache
{
    internal class NothingCached : ICache
    {
        public void Set<TR>(string key, TR value)
        {
        }

        public TR SetIfAbsent<TR>(string key, TR value)
        {
            return value;
        }

        public TR Compute<TR>(string key, Func<TR> valueFunc)
        {
            return valueFunc.Invoke();
        }

        public TR ComputeIfAbsent<TR>(string key, Func<TR> valueFunc)
        {
            return valueFunc.Invoke();
        }

        public TR Get<TR>(string key)
        {
            throw new KeyNotFoundException();
        }

        public bool Remove(string key)
        {
            return false;
        }
    }
}