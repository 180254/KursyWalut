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

        private IList<int> _availableYears;

        private readonly IDictionary<int, IList<DateTime>> _availableDates
            = new Dictionary<int, IList<DateTime>>();

        private readonly IDictionary<DateTime, IList<ExchangeRate>> _dailyExchangeRates
            = new Dictionary<DateTime, IList<ExchangeRate>>();

        private readonly IDictionary<Tuple<Currency, DateTime>, ExchangeRate> _dailyCurrencyExchangeRate
            = new Dictionary<Tuple<Currency, DateTime>, ExchangeRate>();

        private readonly IDictionary<Tuple<Currency, DateTime, DateTime>, IList<ExchangeRate>> _exchangeRatesHistory
            = new Dictionary<Tuple<Currency, DateTime, DateTime>, IList<ExchangeRate>>();

        public ExchangeRatesCacher(IExchangeRatesProvider exchangeRatesProvider)
        {
            _exchangeRatesProvider = exchangeRatesProvider;
        }

        public async Task<IList<int>> GetAvailableYears()
        {
            _lock.WaitOne();

            try
            {
                return _availableYears ?? (_availableYears = await _exchangeRatesProvider.GetAvailableYears());
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IList<DateTime>> GetAvailableDates(int year)
        {
            return await GetFromDictOrCalculate(_availableDates, year,
                async () => await _exchangeRatesProvider.GetAvailableDates(year));
        }


        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day)
        {
            return await GetFromDictOrCalculate(_dailyExchangeRates, day,
                async () => await _exchangeRatesProvider.GetExchangeRates(day));
        }

        public async Task<ExchangeRate> GetExchangeRate(Currency currency, DateTime day)
        {
            var tuple = Tuple.Create(currency, day);
            return await GetFromDictOrCalculate(_dailyCurrencyExchangeRate, tuple,
                async () => await _exchangeRatesProvider.GetExchangeRate(currency, day));
        }

        public async Task<IList<ExchangeRate>> GetExchangeRateHistory(
            Currency currency, DateTime startDay, DateTime stopDay)
        {
            var tuple = Tuple.Create(currency, startDay, stopDay);
            return await GetFromDictOrCalculate(_exchangeRatesHistory, tuple,
                async () => await _exchangeRatesProvider.GetExchangeRateHistory(currency, startDay, stopDay));
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            return _exchangeRatesProvider.Subscribe(observer);
        }

        private async Task<TV> GetFromDictOrCalculate<TK, TV>(IDictionary<TK, TV> dict, TK key, Func<Task<TV>> supplier)
        {
            _lock.WaitOne();

            try
            {
                if (dict.ContainsKey(key))
                    return dict[key];


                var value = await supplier.Invoke();
                dict.Add(key, value);

                return value;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}