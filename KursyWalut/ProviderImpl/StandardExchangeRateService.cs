using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KursyWalut.Model;
using KursyWalut.Progress;
using KursyWalut.Provider;

namespace KursyWalut.ProviderImpl
{
    internal class StandardExchangeRateService : IExchangeRatesService
    {
        private readonly IExchangeRatesProvider _exchangeRatesProvider;

        public StandardExchangeRateService(IExchangeRatesProvider exchangeRatesProvider)
        {
            _exchangeRatesProvider = exchangeRatesProvider;
        }

        public async Task<IList<int>> GetAvailableYears(IPProgress p)
        {
            return await _exchangeRatesProvider.GetAvailableYears(p);
        }

        public async Task<IList<DateTime>> GetAvailableDays(int year, IPProgress p)
        {
            return await _exchangeRatesProvider.GetAvailableDays(year, p);
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day, IPProgress p)
        {
            return await _exchangeRatesProvider.GetExchangeRates(day, p);
        }

        public async Task<ExchangeRate> GetExchangeRate(Currency currency, DateTime day, IPProgress p)
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

        public async Task<DateTime> GetFirstAvailableDay(IPProgress p)
        {
            var firstYear = (await GetAvailableYears(p.SubPercent(0.00, 0.40))).First();
            var firstDate = (await GetAvailableDays(firstYear, p.SubPercent(0.41, 1.00))).First();
            return firstDate;
        }

        public async Task<DateTime> GetLastAvailableDay(IPProgress p)
        {
            var lastYear = (await GetAvailableYears(p.SubPercent(0.00, 0.40))).Last();
            var lastDate = (await GetAvailableDays(lastYear, p.SubPercent(0.41, 1.00))).Last();
            return lastDate;
        }

        public async Task<IList<ExchangeRate>> GetExchangeRateHistory(
            Currency currency, DateTime startDay, DateTime endDay, IPProgress p)
        {
            if (startDay > endDay)
                throw new ArgumentException("start.day > stop.day");
            if (startDay < await GetFirstAvailableDay(p.SubPercent(0.00, 0.05)))
                throw new ArgumentException("start.day < GetFirstAvailableDay()");
            if (endDay > await GetLastAvailableDay(p.SubPercent(0.06, 0.10)))
                throw new ArgumentException("end.day > GetLastvailableDay()");

            var availableDays = await GetDaysBetweenYears(startDay.Year, endDay.Year, p.SubPercent(0.11, 0.40));
            var properDays = availableDays.Where(day => (day >= startDay) && (day <= endDay)).ToImmutableList();
            var exchangeRates = await GetExchangeRatesInDays(properDays, currency, p.SubPercent(0.41, 1.00));

            p.ReportProgress(1.00);
            return exchangeRates;
        }

        private async Task<IList<DateTime>> GetDaysBetweenYears(int startYear, int endYear, IPProgress p)
        {
            var years = Enumerable.Range(startYear, endYear - startYear + 1).ToImmutableList();
            var work = new Task<IList<DateTime>>[years.Count];

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

        private async Task<IList<ExchangeRate>> GetExchangeRatesInDays(
            IList<DateTime> days, Currency currency, IPProgress p)
        {
            var work = new List<Task<ExchangeRate>>();
            var waitFor = Environment.ProcessorCount*10;
            var minWork = waitFor*2/3;

            for (var i = 0; i < days.Count; i++)
            {
                var day = days[i];
                var progress = p.SubPart(i, days.Count);
                work.Add(GetExchangeRate(currency, day, progress));

                SpinWait.SpinUntil(() => work.Count(w => !w.IsCompleted) < minWork);
//                if (i%waitFor == 0) await Task.WhenAll(work);
                if (i%(days.Count/10) == 0) Debug.WriteLine("DL-" + i);
            }

            var workDone = await Task.WhenAll(work);
            p.ReportProgress(1.00);
            return workDone.ToImmutableList();
        }
    }
}