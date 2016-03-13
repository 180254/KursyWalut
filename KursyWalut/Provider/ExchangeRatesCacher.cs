using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KursyWalut.Provider
{
    internal class ExchangeRatesCacher : IExchangeRatesProvider
    {
        private readonly Semaphore _lock = new Semaphore(1, 1);
        private readonly IExchangeRatesProvider _exchangeRatesProvider;

        private IList<DateTime> _availableDates;

        private readonly IDictionary<DateTime, IList<ExchangeRate>> _dailyExchangeRates
            = new Dictionary<DateTime, IList<ExchangeRate>>();

        private readonly IDictionary<Tuple<Currency, DateTime, DateTime>, IList<ExchangeRate>> _exchangeRatesHistory
            = new Dictionary<Tuple<Currency, DateTime, DateTime>, IList<ExchangeRate>>();

        public ExchangeRatesCacher(IExchangeRatesProvider exchangeRatesProvider)
        {
            _exchangeRatesProvider = exchangeRatesProvider;
        }

        public async Task<IList<DateTime>> GetAvailableDates()
        {
            _lock.WaitOne();

            try
            {
                return _availableDates ?? (_availableDates = await _exchangeRatesProvider.GetAvailableDates());
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day)
        {
            _lock.WaitOne();

            try
            {
                if (_dailyExchangeRates.ContainsKey(day))
                    return _dailyExchangeRates[day];

                var exchangeRates = await _exchangeRatesProvider.GetExchangeRates(day);
                _dailyExchangeRates.Add(day, exchangeRates);

                return exchangeRates;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IList<ExchangeRate>> GetExchangeRatesHistory(
            Currency currency, DateTime startDay, DateTime stopDay)
        {
            _lock.WaitOne();

            try
            {
                var tuple = Tuple.Create(currency, startDay, stopDay);

                if (_exchangeRatesHistory.ContainsKey(tuple))
                    return _exchangeRatesHistory[tuple];

                var exchangeRates = await _exchangeRatesProvider.GetExchangeRatesHistory(currency, startDay, stopDay);
                _exchangeRatesHistory.Add(tuple, exchangeRates);

                return exchangeRates;
            }
            finally
            {
                _lock.Release();
            }
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            return _exchangeRatesProvider.Subscribe(observer);
        }
    }
}