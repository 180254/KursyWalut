using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace KursyWalut.Cache
{
#pragma warning disable 1998 // Async function without await expression.
    public class InMemCache : ICache
    {
        private readonly ConcurrentDictionary<string, object> _dict = new ConcurrentDictionary<string, object>();

        public async Task<T> Get<T>(string key)
        {
            object value;
            if (_dict.TryGetValue(key, out value))
            {
                return (T) value;
            }

            return default(T);
        }

        public async Task Store<T>(string key, T value)
        {
            _dict.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }

        public async Task<bool> Remove(string key)
        {
            object ignored;
            return _dict.TryRemove(key, out ignored);
        }

        public async Task Clear()
        {
            _dict.Clear();
        }
    }
#pragma warning restore 1998
}