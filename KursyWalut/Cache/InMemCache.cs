using System;
using System.Collections.Generic;

namespace KursyWalut.Cache
{
    internal class InMemCache : ICache
    {
        private readonly IDictionary<string, object> _dict = new Dictionary<string, object>();

        public T Get<T>(string key, Func<T> default_)
        {
            object value;
            if (_dict.TryGetValue(key, out value))
            {
                return (T) value;
            }

            return default_.Invoke();
        }

        public void Store<T>(string key, T value)
        {
            Remove(key);
            _dict.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        public void Clear()
        {
            _dict.Clear();
        }
    }
}