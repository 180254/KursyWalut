using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace KursyWalut.Cache
{
    public class InMemCache : ICache
    {
        private readonly ConcurrentDictionary<string, object> _dict = new ConcurrentDictionary<string, object>();

        public async Task<T> Get<T>(string key, Func<T> default_)
        {
            object value;
            if (_dict.TryGetValue(key, out value))
            {
                return (T) value;
            }

            return await Task.Run(() => default_.Invoke());
        }

        public async Task Store<T>(string key, T value)
        {
            await Task.Run(() => _dict.AddOrUpdate(key, value, (oldkey, oldvalue) => value));
        }

        public async Task<bool> Remove(string key)
        {
            object ignored;
            return await Task.Run(() => _dict.TryRemove(key, out ignored));
        }

        public async Task Clear()
        {
            await Task.Run(() => _dict.Clear());
        }
    }
}