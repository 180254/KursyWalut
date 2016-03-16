using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursyWalut.Provider
{
    internal interface IExchangeRatesProvider : IObservable<int>
    {
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<int>> GetAvailableYears(Progress p);

        /// <exception cref="T:System.ArgumentException">Year was not returned by GetAvailableYears().</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<DateTime>> GetAvailableDays(int year, Progress p);

        /// <exception cref="T:System.ArgumentException">Day was not returned by GetAvailableDays(day.year).</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<ExchangeRate>> GetExchangeRates(DateTime day, Progress p);
    }
}