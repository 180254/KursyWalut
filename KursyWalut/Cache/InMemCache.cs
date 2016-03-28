using System;
using System.Collections.Concurrent;

namespace KursyWalut.Cache
{
    public class InMemCache : ICache
    {
        private readonly ConcurrentDictionary<string, object> _dict = new ConcurrentDictionary<string, object>();

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
            _dict.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }

        public bool Remove(string key)
        {
            object ignored;
            return _dict.TryRemove(key, out ignored);
        }

        public void Clear()
        {
            _dict.Clear();
        }
    }
}