using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursyWalut.Provider
{
    internal interface IExchangeRatesProvider : IObservable<int>
    {
        Task<IList<DateTime>> GetAvailableDates();
        Task<IList<ExchangeRate>> GetExchangeRates(DateTime day);
    }
}