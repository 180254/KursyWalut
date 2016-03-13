using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursyWalut.Provider
{
    internal interface IExchangeRatesProvider : IObservable<int>
    {
        Task<IList<DateTime>> GetAvailableDates();
        Task<IList<ExchangeRate>> GetExchangeRates(DateTime day);
        Task<IList<ExchangeRate>> GetExchangeRatesHistory(Currency currency, DateTime startDay, DateTime stopDay);
    }
}