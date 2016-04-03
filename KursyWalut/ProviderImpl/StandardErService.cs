using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cimbalino.Toolkit.Extensions;
using KursyWalut.Cache;
using KursyWalut.Extensions;
using KursyWalut.Model;
using KursyWalut.Progress;
using KursyWalut.Provider;

namespace KursyWalut.ProviderImpl
{
    public class StandardErService : IErService, ICacheable
    {
        private readonly IErProvider _exchangeRatesProvider;

        public StandardErService(IErProvider exchangeRatesProvider)
        {
            _exchangeRatesProvider = exchangeRatesProvider;
        }

        public async Task InitCache(IPProgress p)
        {
            var initCache = (_exchangeRatesProvider as ICacheable)?.InitCache(p);
            if (initCache != null)
                await initCache;

            p.ReportProgress(1.00);
        }


        public async Task FlushCache(IPProgress p)
        {
            var flushCache = (_exchangeRatesProvider as ICacheable)?.FlushCache(p);
            if (flushCache != null)
                await flushCache;

            p.ReportProgress(1.00);
        }

        public async Task<IList<int>> GetAvailableYears(IPProgress p)
        {
            return await _exchangeRatesProvider.GetAvailableYears(p);
        }

        public async Task<IList<DateTimeOffset>> GetAvailableDays(int year, IPProgress p)
        {
            return await _exchangeRatesProvider.GetAvailableDays(year, p);
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTimeOffset day, IPProgress p)
        {
            return await _exchangeRatesProvider.GetExchangeRates(day, p);
        }

        public async Task<ExchangeRate> GetExchangeRate(Currency currency, DateTimeOffset day, IPProgress p)
        {
            var exchangeRates = await GetExchangeRates(day, p);
            return exchangeRates.FirstOrDefault(e => e.Currency.Equals(currency));
        }

        public async Task<DateTimeOffset> GetFirstAvailableDay(IPProgress p)
        {
            var firstYear = (await GetAvailableYears(p.SubPercent(0.00, 0.40))).First();
            var firstDate = (await GetAvailableDays(firstYear, p.SubPercent(0.40, 1.00))).First();
            p.ReportProgress(1.00);
            return firstDate;
        }

        public async Task<DateTimeOffset> GetLastAvailableDay(IPProgress p)
        {
            var lastYear = (await GetAvailableYears(p.SubPercent(0.00, 0.40))).Last();
            var lastDate = (await GetAvailableDays(lastYear, p.SubPercent(0.40, 1.00))).Last();
            p.ReportProgress(1.00);
            return lastDate;
        }

        public async Task<IList<DateTimeOffset>> GetAllAvailablesDay(IPProgress p)
        {
            var firstYear = (await GetAvailableYears(p.SubPercent(0.00, 0.05))).First();
            var lastYear = (await GetAvailableYears(p.SubPercent(0.05, 0.10))).Last();
            var availableDays = await GetDaysBetweenYears(firstYear, lastYear, p.SubPercent(0.10, 1.00));
            p.ReportProgress(1.00);
            return availableDays;
        }

        public async Task GetExchangeRateAveragedHistory(
            Currency currency, DateTimeOffset startDay, DateTimeOffset endDay,
            ICollection<ExchangeRate> outErs, int expectedSize, IPProgress p)
        {
            if (startDay > endDay)
                throw new ArgumentException("start.day > stop.day");
            if (startDay < await GetFirstAvailableDay(p.SubPercent(0.00, 0.05)))
                throw new ArgumentException("start.day < GetFirstAvailableDay()");
            if (endDay > await GetLastAvailableDay(p.SubPercent(0.05, 0.10)))
                throw new ArgumentException("end.day > GetLastvailableDay()");

            var availableDays = await GetDaysBetweenYears(startDay.Year, endDay.Year, p.SubPercent(0.10, 0.20));
            var properDays = availableDays
                .Where(day => (day >= startDay) && (day <= endDay))
                .Averaged(expectedSize)
                .ToImmutableList();

            await GetExchangeRatesInDays(properDays, currency, outErs, p.SubPercent(0.20, 1.00));
            p.ReportProgress(1.00);
        }


        private async Task<IList<DateTimeOffset>> GetDaysBetweenYears(int startYear, int endYear, IPProgress p)
        {
            var years = Enumerable.Range(startYear, endYear - startYear + 1).ToImmutableList();
            var work = new Task<IList<DateTimeOffset>>[years.Count];

            for (var i = 0; i < years.Count; i++)
            {
                var t = years[i];
                var progress = p.SubPart(i, years.Count);
                work[i] = GetAvailableDays(t, progress);
            }

            var workDone = await Task.WhenAll(work);
            p.ReportProgress(1.00);

            return workDone.SelectMany(x => x).ToImmutableList();
        }

        private async Task GetExchangeRatesInDays(
            IList<DateTimeOffset> days, Currency currency,
            ICollection<ExchangeRate> ourErs, IPProgress p)
        {
            var work = new List<Task<ExchangeRate>>();
            var waitFor = Environment.ProcessorCount*10;

            for (var i = 0; i < days.Count; i++)
            {
                var day = days[i];
                var progress = p.SubPart(i, days.Count);
                var isCheckpoint = (i%waitFor == 0) || (i == days.Count - 1);

                work.Add(GetExchangeRate(currency, day, progress));

                if (isCheckpoint)
                {
                    await AwaitDoneAndFill(work, ourErs);
                    work.Clear();
                    p.ReportProgress((i + 1.0)/days.Count);
                }

                if ((days.Count > 10) && (i%(days.Count/10) == 0))
                    Debug.WriteLine("DL-{0}-{1}", days.Count, i);
            }
        }

        private static async Task AwaitDoneAndFill<T>(
            IEnumerable<Task<T>> work,
            ICollection<T> @out)
        {
            var workT = await Task.WhenAll(work);
            var nonNullT = workT.Where(c => !ReferenceEquals(c, null));
            @out.AddRange(nonNullT);
        }
    }
}