using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KursyWalut.Cache;
using KursyWalut.Model;
using KursyWalut.Progress;
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

        public async Task<IList<int>> GetAvailableYears(IPProgress p)
        {
            return await Task.Run(() =>
                _cache.Get("_availYears", async () => await _exchangeRatesProvider.GetAvailableYears(p)));
        }

        public async Task<IList<DateTime>> GetAvailableDays(int year, IPProgress p)
        {
            return await GetOrCalculate(
                nameof(_yearToDays), _yearToDays, year,
                () => _exchangeRatesProvider.GetAvailableDays(year, p), p);
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day, IPProgress p)
        {
            return await GetOrCalculate(
                nameof(_dayToEr), _dayToEr, day,
                () => _exchangeRatesProvider.GetExchangeRates(day, p), p);
        }

        private async Task<TV> GetOrCalculate<TK, TV>(
            string cacheKey, IDictionary<TK, TV> dict,
            TK key, Func<Task<TV>> valueSup,
            IPProgress p)
        {
            if (dict.ContainsKey(key))
            {
                p.ReportProgress(1.00);
                return dict[key];
            }

            var value = await valueSup.Invoke();
            dict.Add(key, value);
            _cache.Store(cacheKey, dict);
            return value;
        }
    }
}