using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursyWalut.Provider
{
    internal interface IExchangeRatesProvider : IObservable<int>
    {
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<int>> GetAvailableYears();

        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<DateTime>> GetAvailableDates(int year);

        /// <exception cref="T:System.ArgumentException">Day was not returned by GetAvailableDates(int).</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<ExchangeRate>> GetExchangeRates(DateTime day);

        /// <exception cref="T:System.ArgumentException">Invalid currency.</exception>
        /// <exception cref="T:System.ArgumentException">Day was not returned by GetAvailableDates(int).</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<ExchangeRate> GetExchangeRate(Currency currency, DateTime day);

        /// <exception cref="T:System.ArgumentException">Invalid currency.</exception>
        /// <exception cref="T:System.ArgumentException">Start year was not returned by GetAvailableYears().</exception>
        /// <exception cref="T:System.ArgumentException">End year was not returned by GetAvailableYears().</exception>
        /// <exception cref="T:System.ArgumentException">Start day > stop day.</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        Task<IList<ExchangeRate>> GetExchangeRateHistory(Currency currency, DateTime startDay, DateTime stopDay);
    }
}