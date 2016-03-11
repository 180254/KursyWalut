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

        public ExchangeRatesCacher(IExchangeRatesProvider exchangeRatesProvider)
        {
            _exchangeRatesProvider = exchangeRatesProvider;
        }

        public async Task<IList<DateTime>> GetAvailableDates()
        {
            _lock.WaitOne();

            if (_availableDates == null)
                _availableDates = await _exchangeRatesProvider.GetAvailableDates();

            _lock.Release();
            return _availableDates;
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day)
        {
            _lock.WaitOne();

            if (_dailyExchangeRates.ContainsKey(day))
                return _dailyExchangeRates[day];

            var exchangeRates = await _exchangeRatesProvider.GetExchangeRates(day);
            _dailyExchangeRates.Add(day, exchangeRates);

            _lock.Release();
            return exchangeRates;
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            return _exchangeRatesProvider.Subscribe(observer);
        }
    }
}