using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace KursyWalut.Provider.Impl
{
    internal class StandardExchangeRateService : IExchangeRatesService
    {
        private readonly IExchangeRatesProvider _exchangeRatesProvider;

        public StandardExchangeRateService(IExchangeRatesProvider exchangeRatesProvider)
        {
            _exchangeRatesProvider = exchangeRatesProvider;
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            return _exchangeRatesProvider.Subscribe(observer);
        }

        public async Task<IList<int>> GetAvailableYears(Progress p)
        {
            return await _exchangeRatesProvider.GetAvailableYears(p);
        }

        public async Task<IList<DateTime>> GetAvailableDays(int year, Progress p)
        {
            return await _exchangeRatesProvider.GetAvailableDays(year, p);
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day, Progress p)
        {
            return await _exchangeRatesProvider.GetExchangeRates(day, p);
        }

        public async Task<ExchangeRate> GetExchangeRate(Currency currency, DateTime day, Progress p)
        {
            try
            {
                var exchangeRates = await GetExchangeRates(day, p);
                return exchangeRates.First(e => e.Currency.Equals(currency)); // possible InvalidOperationException
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                throw new ArgumentException("invalid currency");
            }
        }

        public async Task<DateTime> GetFirstAvailableDay(Progress p)
        {
            var firstYear = (await GetAvailableYears(p.Partial(0, 0.1))).First();
            var firstDate = (await GetAvailableDays(firstYear, p.Partial(0.11, 1))).First();
            return firstDate;
        }

        public async Task<DateTime> GetLastAvailableDay(Progress p)
        {
            var lastYear = (await GetAvailableYears(p.Partial(0, 0.1))).Last();
            var lastDate = (await GetAvailableDays(lastYear, p.Partial(0.11, 1))).Last();
            return lastDate;
        }

        public async Task<IList<ExchangeRate>> GetExchangeRateHistory(
            Currency currency, DateTime startDay, DateTime stopDay, Progress p)
        {
            var startYear = startDay.Year;
            var stopYear = stopDay.Year;

            var availableDays = new List<DateTime>();
            var years = Enumerable.Range(startYear, stopYear - startYear + 1).ToImmutableList();
            var progress = p.Partial(0, 0.4);
            for (var i = 0; i < years.Count; i++)
            {
                var t = years[i];
                var progress2 = progress.Partial2(i, years.Count);
                availableDays.AddRange(await GetAvailableDays(t, progress2));
            }

            var exchangeRates = new List<ExchangeRate>();
            var days = availableDays.Where(day => (day >= startDay) && (day <= stopDay)).ToImmutableList();
            progress = p.Partial(0.41, 1);
            for (var i = 0; i < days.Count; i++)
            {
                var day = days[i];
                var progress2 = progress.Partial2(i, days.Count);
                exchangeRates.Add(await GetExchangeRate(currency, day, progress2));
            }
            return exchangeRates;
        }
    }
}