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
            IList<ExchangeRate> exchangeRates = null;
            try
            {
                exchangeRates = await GetExchangeRates(day, p);
                return exchangeRates.First(e => e.Currency.Equals(currency)); // possible InvalidOperationException
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                throw new ArgumentException("invalid currency; want " + currency + ";" +
                                            "day " + day + "; " +
                                            "avail " +
                                            string.Join<Currency>(",", exchangeRates?.Select(e => e.Currency).ToArray()));
            }
        }

        public async Task<DateTime> GetFirstAvailableDay(Progress p)
        {
            var firstYear = (await GetAvailableYears(p.PartialPercent(0.00, 0.40))).First();
            var firstDate = (await GetAvailableDays(firstYear, p.PartialPercent(0.41, 1.00))).First();
            return firstDate;
        }

        public async Task<DateTime> GetLastAvailableDay(Progress p)
        {
            var lastYear = (await GetAvailableYears(p.PartialPercent(0.00, 0.40))).Last();
            var lastDate = (await GetAvailableDays(lastYear, p.PartialPercent(0.41, 1.00))).Last();
            return lastDate;
        }

        public async Task<IList<ExchangeRate>> GetExchangeRateHistory(
            Currency currency, DateTime startDay, DateTime endDay, Progress p)
        {
            if (startDay > endDay)
                throw new ArgumentException("start.day > stop.day");
            if (startDay < await GetFirstAvailableDay(p.PartialPercent(0.00, 0.05)))
                throw new ArgumentException("start.day < GetFirstAvailableDay()");
            if (endDay > await GetLastAvailableDay(p.PartialPercent(0.06, 0.10)))
                throw new ArgumentException("end.day > GetLastvailableDay()");

            var availableDays = await GetDaysBetweenYears(startDay.Year, endDay.Year, p.PartialPercent(0.11, 0.40));
            var properDays = availableDays.Where(day => (day >= startDay) && (day <= endDay)).ToImmutableList();
            var exchangeRates = await GetExchangeRatesInDays(properDays, currency, p.PartialPercent(0.41, 1.00));

            return exchangeRates;
        }

        private async Task<List<DateTime>> GetDaysBetweenYears(int startYear, int endYear, Progress p)
        {
            var years = Enumerable.Range(startYear, endYear - startYear + 1).ToImmutableList();

            var availableDays = new List<DateTime>();
            for (var i = 0; i < years.Count; i++)
            {
                var t = years[i];
                var progress = p.PartialPart(i, years.Count);
                availableDays.AddRange(await GetAvailableDays(t, progress));
            }

            return availableDays;
        }

        private async Task<List<ExchangeRate>> GetExchangeRatesInDays(
            IList<DateTime> days, Currency currency, Progress p)
        {
            var exchangeRates = new List<ExchangeRate>();
            for (var i = 0; i < days.Count; i++)
            {
                var day = days[i];
                var progress = p.PartialPart(i, days.Count);

                try
                {
                    // possible ArgumentException(invalid/unknown currency)
                    exchangeRates.Add(await GetExchangeRate(currency, day, progress));
                }
                catch (Exception ex) when (ex is ArgumentException)
                {
                }
            }

            return exchangeRates;
        }
    }
}