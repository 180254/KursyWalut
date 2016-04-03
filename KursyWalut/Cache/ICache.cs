using System.Threading.Tasks;

namespace KursyWalut.Cache
{
    public interface ICache
    {
        /// Info: returns default(T) if not cached.
        /// <exception cref="T:System.InvalidCastException">Value for <paramref name="key" /> is not of type T.</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<T> Get<T>(string key);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task Store<T>(string key, T value);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<bool> Remove(string key);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task Clear();
    }
}