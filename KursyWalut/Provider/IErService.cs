using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KursyWalut.Model;
using KursyWalut.Progress;

namespace KursyWalut.Provider
{
    public interface IErService : IErProvider
    {
        /// Info: returns default(Currency) if no such currency in given day.
        /// <exception cref="T:System.ArgumentException">Day was not returned by GetAvailableDays(day.year).</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<ExchangeRate> GetExchangeRate(Currency currency, DateTimeOffset day, IPProgress p);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<DateTimeOffset> GetFirstAvailableDay(IPProgress p);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<DateTimeOffset> GetLastAvailableDay(IPProgress p);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<DateTimeOffset>> GetAllAvailablesDay(IPProgress p);

        /// Info: expectedSize is approx. May be less if not enough data, or greater (if so decided by the algorithm).
        /// <exception cref="T:System.ArgumentException">Start.day &gt; stop.day.</exception>
        /// <exception cref="T:System.ArgumentException">Start.day &lt; GetFirstAvailableDay().</exception>
        /// <exception cref="T:System.ArgumentException">End.day &gt; GetLastvailableDay().</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task GetExchangeRateAveragedHistory(
            Currency currency, DateTimeOffset startDay, DateTimeOffset endDay,
            ICollection<ExchangeRate> outErs, int expectedSize, IPProgress p);
    }
}