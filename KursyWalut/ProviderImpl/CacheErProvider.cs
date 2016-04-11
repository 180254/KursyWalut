using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KursyWalut.Cache;
using KursyWalut.Model;
using KursyWalut.Progress;
using KursyWalut.Provider;

namespace KursyWalut.ProviderImpl
{
    public class CacheErProvider : IErProvider, ICacheable
    {
        private readonly IErProvider _exchangeRatesProvider;
        private readonly ICache _cache;

        private IList<int> _availYears;
        private IDictionary<int, IList<DateTimeOffset>> _yearToDays;
        private IDictionary<DateTimeOffset, IList<ExchangeRate>> _dayToEr;

        private readonly IDictionary<string, bool> _cacheChanged = new Dictionary<string, bool>();

        public CacheErProvider(IErProvider exchangeRatesProvider, ICache cache)
        {
            _exchangeRatesProvider = exchangeRatesProvider;
            _cache = cache;

            ResetCacheChanges();
        }

        public async Task InitCache(IPProgress p)
        {
            _availYears = await _cache.Get<IList<int>>(nameof(_availYears));
            p.ReportProgress(0.10);

            _yearToDays =
                await _cache.Get<IDictionary<int, IList<DateTimeOffset>>>(nameof(_yearToDays))
                ?? new Dictionary<int, IList<DateTimeOffset>>();
            p.ReportProgress(0.20);

            _dayToEr =
                await _cache.Get<IDictionary<DateTimeOffset, IList<ExchangeRate>>>(nameof(_dayToEr))
                ?? new Dictionary<DateTimeOffset, IList<ExchangeRate>>();
            p.ReportProgress(0.50);

            var initCache = (_exchangeRatesProvider as ICacheable)?.InitCache(p.SubPercent(0.50, 1.00));
            if (initCache != null)
                await initCache;

            p.ReportProgress(1.00);
        }

        public async Task FlushCache(IPProgress p)
        {
            if (_cacheChanged[nameof(_availYears)])
                await _cache.Store(nameof(_availYears), _availYears);
            p.ReportProgress(0.10);

            if (_cacheChanged[nameof(_yearToDays)])
                await _cache.Store(nameof(_yearToDays), _yearToDays);
            p.ReportProgress(0.20);

            if (_cacheChanged[nameof(_dayToEr)])
                await _cache.Store(nameof(_dayToEr), _dayToEr);
            p.ReportProgress(0.50);

            var flushCache = (_exchangeRatesProvider as ICacheable)?.FlushCache(p.SubPercent(0.50, 1.00));
            if (flushCache != null)
                await flushCache;

            ResetCacheChanges();
            p.ReportProgress(1.00);
        }

        public async Task<IList<int>> GetAvailableYears(IPProgress p)
        {
            return _availYears ?? (_availYears = await CalculateAvailableYears(p));
        }

        public async Task<IList<DateTimeOffset>> GetAvailableDays(int year, IPProgress p)
        {
            return await GetOrCalculate(nameof(_yearToDays), _yearToDays, year,
                () => _exchangeRatesProvider.GetAvailableDays(year, p), p);
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTimeOffset day, IPProgress p)
        {
            return await GetOrCalculate(nameof(_dayToEr), _dayToEr, day,
                () => _exchangeRatesProvider.GetExchangeRates(day, p), p);
        }

        private async Task<IList<int>> CalculateAvailableYears(IPProgress p)
        {
            _cacheChanged[nameof(_availYears)] = true;
            return await _exchangeRatesProvider.GetAvailableYears(p);
        }

        private async Task<TV> GetOrCalculate<TK, TV>(
            string dictName, IDictionary<TK, TV> dict,
            TK key, Func<Task<TV>> valueSup,
            IPProgress p)
        {
            if (dict.ContainsKey(key))
            {
                p.ReportProgress(1.00);
                return dict[key];
            }

            _cacheChanged[dictName] = true;
            var value = await valueSup.Invoke();
            dict.Add(key, value);
            return value;
        }

        private void ResetCacheChanges()
        {
            _cacheChanged[nameof(_availYears)] = false;
            _cacheChanged[nameof(_yearToDays)] = false;
            _cacheChanged[nameof(_dayToEr)] = false;
        }
    }
}