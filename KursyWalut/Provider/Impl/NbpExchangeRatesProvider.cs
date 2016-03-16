using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KursyWalut.Provider.Impl
{
    internal class NbpExchangeRatesProvider : IExchangeRatesProvider
    {
        private readonly Semaphore _lock = new Semaphore(1, 1);
        private readonly Encoding _utf8;
        private readonly Encoding _iso88592;

        // -----------------------------------------------------------------------------------------

        private readonly IList<IObserver<int>> _observers;
        private readonly IDictionary<int, IList<DateTime>> _yearToDays;
        private readonly IDictionary<DateTime, string> _dayToFilename;
        private readonly IDictionary<string, IList<ExchangeRate>> _filenameToEr;

        // -----------------------------------------------------------------------------------------

        public NbpExchangeRatesProvider()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _utf8 = Encoding.UTF8;
            _iso88592 = Encoding.GetEncoding("ISO-8859-2");

            _observers = new List<IObserver<int>>();

            _yearToDays = new Dictionary<int, IList<DateTime>>();
            _dayToFilename = new Dictionary<DateTime, string>();
            _filenameToEr = new Dictionary<string, IList<ExchangeRate>>();
        }

        // -----------------------------------------------------------------------------------------

        public IDisposable Subscribe(IObserver<int> observer)
        {
            _observers.Add(observer);
            return new ObserverDisposable(this, observer);
        }

        // -----------------------------------------------------------------------------------------

        public async Task<IList<int>> GetAvailableYears()
        {
            return await GetAvailableYears2(Progress.Master);
        }

        public async Task<IList<DateTime>> GetAvailableDays(int year)
        {
            return await GetAvailableDates2(year, Progress.Master);
        }

        public  Task<DateTime> GetFirstAvailableDate()
        {
            throw new NotImplementedException();
        }

        public  Task<DateTime> GetLastAvailableDate()
        {
            throw new NotImplementedException();
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day)
        {
            return await GetExchangeRates2(day, Progress.Master);
        }

        public async Task<ExchangeRate> GetExchangeRate(Currency currency, DateTime day)
        {
            return await GetExchangeRate2(currency, day, Progress.Master);
        }

        public async Task<IList<ExchangeRate>> GetExchangeRateHistory(
            Currency currency, DateTime startDay, DateTime stopDay)
        {
            return await GetExchangeRateHistory2(currency, startDay, stopDay, Progress.Master);
        }

        // -----------------------------------------------------------------------------------------

        /// internal version of GetAvailableYears; customizable lock, and progress start/stop value
        private async Task<IList<int>> GetAvailableYears2(Progress p)
        {
            const int startYear = 2002;
            var endYear = DateTime.Now.Year;
            return await Task.Run(() => Enumerable.Range(startYear, endYear - startYear +1).ToImmutableList());
        }

        /// internal version of GetAvailableDate; customizable lock, and progress start/stop value
        private async Task<IList<DateTime>> GetAvailableDates2(int year, Progress p)
        {
            if (p.IsMaster) _lock.WaitOne();

            try
            {
                NotifyObservers(p.Start);

                if (_yearToDays.ContainsKey(year))
                {
                    NotifyObservers(p.End);
                    return _yearToDays[year];
                }

                var partProgress = CalculateProgress(p, 10);
                var availableYears = await GetAvailableYears2(new Progress(p.Start, partProgress));
                var first = availableYears.First(y => y.Equals(year)); // possible InvalidOperationException
                NotifyObservers(partProgress);

                var yearS = year == DateTime.Now.Year ? "" : year.ToString();
                var dir = await GetHttpResponse("http://www.nbp.pl/kursy/xml/dir" + yearS + ".txt", _utf8);
                var filenames = ParseFilenames(dir);
                NotifyObservers(CalculateProgress(p, 70));

                var result = new List<DateTime>();
                foreach (var filename in filenames.Where(f => f.StartsWith("a")))
                {
                    var day = ParseDateTime(filename);
                    _dayToFilename.Add(day, filename);
                    result.Add(day);
                }

                _yearToDays.Add(year, result);
                NotifyObservers(p.End);

                return result.AsReadOnly();
            }

            catch (Exception ex) when (ex is InvalidOperationException)
            {
                if (p.IsMaster) NotifyObservers(-1);
                throw new ArgumentException("year was not returned by GetAvailableYears()");
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is IOException))
            {
                if (p.IsMaster) NotifyObservers(-1);
                throw new IOException("response unavailable or in unexpected format", ex);
            }
            finally
            {
                if (p.IsMaster) _lock.Release();
            }
        }

        /// internal version of GetExchangeRates; customizable lock, and progress start/stop value
        private async Task<IList<ExchangeRate>> GetExchangeRates2(DateTime day, Progress p)
        {
            if (p.IsMaster) _lock.WaitOne();

            try
            {
                NotifyObservers(p.Start);

                if (_dayToFilename.ContainsKey(day))
                    if (_filenameToEr.ContainsKey(_dayToFilename[day]))
                    {
                        NotifyObservers(p.End);
                        return _filenameToEr[_dayToFilename[day]];
                    }

                var partProgress = CalculateProgress(p, 30);
                await GetAvailableDates2(day.Year, new Progress(p.Start, partProgress));
                NotifyObservers(partProgress);

                var filename = _dayToFilename[day]; // possible KeyNotFoundException
                var response = await GetHttpResponse("http://www.nbp.pl/kursy/xml/" + filename + ".xml", _iso88592);
                var xml = XDocument.Load(new StringReader(response));
                NotifyObservers(CalculateProgress(p, 70));

                var exchangeRates = ParseExchangeRates(xml, day);
                _filenameToEr.Add(filename, exchangeRates);
                NotifyObservers(p.End);

                return exchangeRates;
            }

            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                if (p.IsMaster) NotifyObservers(-1);
                throw new ArgumentException("day was not returned by GetAvailableDays(day.year)");
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is IOException))
            {
                if (p.IsMaster) NotifyObservers(-1);
                throw new IOException("response unavailable or in unexpected format", ex);
            }
            finally
            {
                if (p.IsMaster) _lock.Release();
            }
        }

        /// internal version of GetExchangeRate; customizable lock, and progress start/stop value
        private async Task<ExchangeRate> GetExchangeRate2(Currency currency, DateTime day, Progress p)
        {
            if (p.IsMaster) _lock.WaitOne();

            try
            {
                NotifyObservers(p.Start);

                var partProgress = CalculateProgress(p, 70);
                var exchangeRates2 = await GetExchangeRates2(day, new Progress(p.Start, partProgress));

                NotifyObservers(partProgress);
                var exchangeRate = exchangeRates2.First(e => e.Currency.Equals(currency)); // possible InvalidOperationException

                NotifyObservers(p.End);
                return exchangeRate;
            }

            catch (Exception ex) when (ex is InvalidOperationException)
            {
                if (p.IsMaster) NotifyObservers(-1);
                throw new ArgumentException("invalid currency");
            }
            finally
            {
                if (p.IsMaster) _lock.Release();
            }
        }

        /// internal version of GetExchangeRatesHistory; customizable lock, and progress start/stop value
        private async Task<IList<ExchangeRate>> GetExchangeRateHistory2(
            Currency currency, DateTime startDay, DateTime stopDay, Progress p)
        {
            if (p.IsMaster) _lock.WaitOne();

            try
            {
                NotifyObservers(p.Start);

                var progress = p.Start;

                var startYear = startDay.Year;
                var stopYear = stopDay.Year;
                var partProgress = CalculateProgress(p, 30);
                var progressPerYear = (partProgress - p.Start)/(stopYear - startYear + 1);
                for (var year = startYear; year <= stopYear; year++)
                {
                    await GetAvailableDates2(year, new Progress(progress, progress + progressPerYear));
                    progress += progressPerYear;
                    NotifyObservers(progress);
                }
                NotifyObservers(partProgress);

                var days = _dayToFilename
                    .Where(s => s.Key >= startDay && s.Key <= stopDay)
                    .Select(s => s.Key)
                    .OrderBy(day => day);

                var progressPerDay = (p.End - progress)/days.Count();
                var result = new List<ExchangeRate>();
                foreach (var dateTime in days)
                {
                    var exchangeRates =
                        await GetExchangeRates2(dateTime, new Progress(progress, progress + progressPerDay));
                    var exchangeRate = exchangeRates.First(e => e.Currency.Equals(currency));

                    result.Add(exchangeRate);
                    progress += progressPerDay;
                    NotifyObservers(progress);
                }

                return result.AsReadOnly();
            }

            catch (Exception ex) when (!(ex is ArgumentException || ex is IOException))
            {
                if (p.IsMaster) NotifyObservers(-1);
                throw new IOException("response unavailable or in unexpected format", ex);
            }
            finally
            {
                if (p.IsMaster) _lock.Release();
            }
        }

        // -----------------------------------------------------------------------------------------

        /// <exception cref="T:System.Exception">If something go wrong.</exception>
        private async Task<string> GetHttpResponse(string requestUri, Encoding encoding)
        {
            using (var client = new HttpClient())
            {
                var bytes = await client.GetByteArrayAsync(requestUri);
                var str = encoding.GetString(bytes);
                return str;
            }
        }

        /// <exception cref="T:System.FormatException">httpResponse is in invalid format.</exception>
        private IEnumerable<string> ParseFilenames(string httpResponse)
        {
            var filenames = httpResponse.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            if (filenames.Length == 0)
                throw new FormatException("empty response");

            return filenames;
        }

        /// <exception cref="T:System.FormatException">filename is in invalid format.</exception>
        private DateTime ParseDateTime(string filename)
        {
            try
            {
                return DateTime.ParseExact(filename.Substring(5, 6), "yyMMdd", CultureInfo.InvariantCulture);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new FormatException("filename is too short", ex);
            }
        }

        /// <exception cref="T:System.FormatException">xContainer is in invalid format.</exception>
        private IList<ExchangeRate> ParseExchangeRates(XContainer xml, DateTime day)
        {
            var exchangeRates = xml
                .Descendants("tabela_kursow")
                .Descendants("pozycja")
                .Select(x => ParseExchangeRate(x, day));

            var exchangeRatesList = exchangeRates.ToImmutableList();
            if (exchangeRatesList.Count == 0)
                throw new FormatException("empty response");

            return exchangeRatesList;
        }

        /// <exception cref="T:System.FormatException">xContainer is in invalid format.</exception>
        private ExchangeRate ParseExchangeRate(XContainer x, DateTime day)
        {
            try
            {
                return new ExchangeRate(
                    day,
                    ParseCurrency(x),
                    double.Parse(x.Element("kurs_sredni").Value.Replace(",", ".")));
            }
            catch (Exception ex)
                when (ex is NullReferenceException || ex is OverflowException || ex is ArgumentException)
            {
                throw new FormatException("xContainer is in invalid format");
            }
        }

        /// <exception cref="T:System.FormatException">xContainer is in invalid format.</exception>
        private Currency ParseCurrency(XContainer x)
        {
            try
            {
                return new Currency(
                    x.Element("kod_waluty").Value,
                    x.Element("nazwa_waluty").Value,
                    int.Parse(x.Element("przelicznik").Value));
            }
            catch (Exception ex) when (ex is NullReferenceException || ex is OverflowException)
            {
                throw new FormatException("xContainer is in invalid format");
            }
        }

        // -----------------------------------------------------------------------------------------


        private async Task<TV> ComputerIfAbsent<TK, TV>(IDictionary<TK, TV> dict, TK key, Func<Task<TV>> supplier)
        {
            if (dict.ContainsKey(key))
                return dict[key];

            var value = await supplier.Invoke();
            dict.Add(key, value);

            return value;
        }

        // -----------------------------------------------------------------------------------------

        private int CalculateProgress(Progress p, int percentDone)
        {
            return p.Start + (p.End - p.Start)*(percentDone/100);
        }

        private void NotifyObservers(int percentDone)
        {
            foreach (var observer in _observers)
            {
                switch (percentDone)
                {
                    case -1:
                        observer.OnError(null);
                        break;
                    case 100:
                        observer.OnCompleted();
                        break;
                    default:
                        observer.OnNext(percentDone);
                        break;
                }
            }
        }


        private class ObserverDisposable : IDisposable
        {
            private readonly NbpExchangeRatesProvider _parent;
            private readonly IObserver<int> _observer;

            public ObserverDisposable(NbpExchangeRatesProvider parent, IObserver<int> observer)
            {
                _parent = parent;
                _observer = observer;
            }

            public void Dispose()
            {
                _parent._observers.Remove(_observer);
            }
        }
    }
}