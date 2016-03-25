using KursyWalut.Progress;

namespace KursyWalut.Cache
{
    public interface ICacheable
    {
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        void FlushCache(IPProgress p);
    }
}