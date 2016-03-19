using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KursyWalut.Model;
using KursyWalut.Progress;

namespace KursyWalut.Provider
{
    internal interface IExchangeRatesProvider
    {
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<int>> GetAvailableYears(IPProgress p);

        /// <exception cref="T:System.ArgumentException">Year was not returned by GetAvailableYears().</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<DateTime>> GetAvailableDays(int year, IPProgress p);

        /// <exception cref="T:System.ArgumentException">Day was not returned by GetAvailableDays(day.year).</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<ExchangeRate>> GetExchangeRates(DateTime day, IPProgress p);
    }
}