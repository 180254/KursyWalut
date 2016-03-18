using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KursyWalut.Cache;
using KursyWalut.Model;
using KursyWalut.Provider;

namespace KursyWalut.ProviderImpl
{
    internal class CacheExchangeRateProvider : IExchangeRatesProvider
    {
        private readonly IExchangeRatesProvider _exchangeRatesProvider;

        private readonly ICache _cache;
        private readonly IDictionary<int, IList<DateTime>> _yearToDays;
        private readonly IDictionary<DateTime, IList<ExchangeRate>> _dayToEr;

        public CacheExchangeRateProvider(IExchangeRatesProvider exchangeRatesProvider, ICache cache)
        {
            _exchangeRatesProvider = exchangeRatesProvider;

            _cache = cache;
            _yearToDays = cache.Get(nameof(_yearToDays), () => new Dictionary<int, IList<DateTime>>());
            _dayToEr = cache.Get(nameof(_dayToEr), () => new Dictionary<DateTime, IList<ExchangeRate>>());
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            return _exchangeRatesProvider.Subscribe(observer);
        }

        public async Task<IList<int>> GetAvailableYears(Progress p)
        {
            return await Task.Run(() =>
                _cache.Get("_availYears", async () => await _exchangeRatesProvider.GetAvailableYears(p)));
        }

        public async Task<IList<DateTime>> GetAvailableDays(int year, Progress p)
        {
            return await GetOrCalculate(
                nameof(_yearToDays), _yearToDays, year,
                () => _exchangeRatesProvider.GetAvailableDays(year, p));
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day, Progress p)
        {
            return await GetOrCalculate(
                nameof(_dayToEr), _dayToEr, day,
                () => _exchangeRatesProvider.GetExchangeRates(day, p));
        }

        private async Task<TV> GetOrCalculate<TK, TV>(
            string cacheKey, IDictionary<TK, TV> dict,
            TK key, Func<Task<TV>> valueSup)
        {
            if (dict.ContainsKey(key))
                return dict[key];

            var value = await valueSup.Invoke();
            dict.Add(key, value);
            _cache.Store(cacheKey, dict);

            return value;
        }
    }
}