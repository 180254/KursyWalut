using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursyWalut.Provider
{
    internal interface IExchangeRatesProvider : IObservable<int>
    {
        Task<IList<int>> GetAvailableYears();
        Task<IList<DateTime>> GetAvailableDates(int year);

        Task<IList<ExchangeRate>> GetExchangeRates(DateTime day);
        Task<ExchangeRate> GetExchangeRate(Currency currency, DateTime day);

        Task<IList<ExchangeRate>> GetExchangeRateHistory(Currency currency, DateTime startDay, DateTime stopDay);
    }
}