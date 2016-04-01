using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KursyWalut.Model;
using KursyWalut.Progress;

namespace KursyWalut.Provider
{
    public interface IExchangeRatesService : IExchangeRatesProvider
    {
        /// <exception cref="T:System.ArgumentException">Invalid currency.</exception>
        /// <exception cref="T:System.ArgumentException">Day was not returned by GetAvailableDays(day.year).</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<ExchangeRate> GetExchangeRate(Currency currency, DateTime day, IPProgress p);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<DateTime> GetFirstAvailableDay(IPProgress p);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<DateTime> GetLastAvailableDay(IPProgress p);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<DateTime>> GetAllAvailablesDay(IPProgress p);

        /// <exception cref="T:System.ArgumentException">Invalid currency.</exception>
        /// <exception cref="T:System.ArgumentException">Start.day &gt; stop.day.</exception>
        /// <exception cref="T:System.ArgumentException">Start.day &lt; GetFirstAvailableDay().</exception>
        /// <exception cref="T:System.ArgumentException">End.day &gt; GetLastvailableDay().</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task GetExchangeRateHistory(
            Currency currency, DateTime startDay, DateTime endDay,
            ICollection<ExchangeRate> ers, IPProgress p);
    }
}