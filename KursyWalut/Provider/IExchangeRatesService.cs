using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// <exception cref="T:System.ArgumentException">Start.year was not returned by GetAvailableYears().</exception>
        /// <exception cref="T:System.ArgumentException">End.year was not returned by GetAvailableYears().</exception>
        /// <exception cref="T:System.ArgumentException">Start.day &lt; GetFirstAvailableDay().</exception>
        /// <exception cref="T:System.ArgumentException">End.day &gt; GetLastvailableDate().</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<ExchangeRate>> GetExchangeRateHistory(
            Currency currency, DateTime startDay, DateTime stopDay, Progress p);
    }
}