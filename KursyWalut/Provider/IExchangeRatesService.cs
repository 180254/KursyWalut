using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KursyWalut.Model;

namespace KursyWalut.Provider
{
    internal interface IExchangeRatesService : IExchangeRatesProvider
    {
        /// <exception cref="T:System.ArgumentException">Invalid currency.</exception>
        /// <exception cref="T:System.ArgumentException">Day was not returned by GetAvailableDays(day.year).</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<ExchangeRate> GetExchangeRate(Currency currency, DateTime day, Progress p);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<DateTime> GetFirstAvailableDay(Progress p);

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<DateTime> GetLastAvailableDay(Progress p);

        /// <exception cref="T:System.ArgumentException">Invalid currency.</exception>
        /// <exception cref="T:System.ArgumentException">Start.day &gt; stop.day.</exception>
        /// <exception cref="T:System.ArgumentException">Start.day &lt; GetFirstAvailableDay().</exception>
        /// <exception cref="T:System.ArgumentException">End.day &gt; GetLastvailableDay().</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<ExchangeRate>> GetExchangeRateHistory(
            Currency currency, DateTime startDay, DateTime endDay, Progress p);
    }
}