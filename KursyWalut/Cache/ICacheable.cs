using System.Threading.Tasks;
using KursyWalut.Progress;

namespace KursyWalut.Cache
{
    public interface ICacheable
    {
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task InitCache(IPProgress p);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task FlushCache(IPProgress p);
    }
}