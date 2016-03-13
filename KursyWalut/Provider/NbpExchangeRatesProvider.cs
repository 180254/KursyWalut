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

namespace KursyWalut.Provider
{
    internal class NbpExchangeRatesProvider : IExchangeRatesProvider
    {
        private readonly Encoding _utf8;
        private readonly Encoding _iso88592;
        private readonly Semaphore _lock = new Semaphore(1, 1);

        private readonly IList<IObserver<int>> _observers = new List<IObserver<int>>();
        private readonly IDictionary<DateTime, string> _dateToFilename = new Dictionary<DateTime, string>();

        public NbpExchangeRatesProvider()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _utf8 = Encoding.UTF8;
            _iso88592 = Encoding.GetEncoding("ISO-8859-2");
        }

        /// <exception cref="T:System.IO.IOException">Response unavailable or in unexpected format.</exception>
        public async Task<IList<DateTime>> GetAvailableDates()
        {
            return await GetAvailableDates2(ProgressNotify.Master);
        }

        /// <exception cref="T:System.ArgumentException">Day was not returned by GetAvailableDates().</exception>
        /// <exception cref="T:System.IO.IOException">Response unavailable or in unexpected format.</exception>
        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day)
        {
            return await GetExchangeRates2(day, ProgressNotify.Master);
        }

        /// <exception cref="T:System.ArgumentException">????</exception>
        /// <exception cref="T:System.IO.IOException">Response unavailable or in unexpected format.</exception>
        public async Task<IList<ExchangeRate>> GetExchangeRatesHistory(
            Currency currency, DateTime startDay, DateTime stopDay)
        {
            return await GetExchangeRatesHistory2(currency, startDay, stopDay, ProgressNotify.Master);
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            _observers.Add(observer);
            return new ObserverDisposable(this, observer);
        }

        /// internal version of GetAvailableDate; customizable lock, and progress start/stop value
        private async Task<IList<DateTime>> GetAvailableDates2(ProgressNotify p)
        {
            if (p.IsMaster) _lock.WaitOne();

            try
            {
                NotifyObservers(p.Start);

                var dir = await GetHttpResponse("http://www.nbp.pl/kursy/xml/dir.txt", _utf8);
                var filenames = ParseFilenames(dir);

                NotifyObservers(CalculateProgress(p, 70));
                _dateToFilename.Clear();

                var result = new List<DateTime>();
                foreach (var filename in filenames.Where(f => f.StartsWith("a")))
                {
                    var day = ParseDateTime(filename);
                    _dateToFilename.Add(day, filename);
                    result.Add(day);
                }

                NotifyObservers(p.Stop);
                return result.AsReadOnly();
            }

            catch (Exception ex)
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
        private async Task<IList<ExchangeRate>> GetExchangeRates2(DateTime day, ProgressNotify p)
        {
            if (p.IsMaster) _lock.WaitOne();

            try
            {
                NotifyObservers(p.Start);
                var filename = _dateToFilename[day];

                var response = await GetHttpResponse("http://www.nbp.pl/kursy/xml/" + filename + ".xml", _iso88592);
                var xml = XDocument.Load(new StringReader(response));
                NotifyObservers(CalculateProgress(p, 70));

                var exchangeRates = ParseExchangeRates(xml, day);
                NotifyObservers(p.Stop);

                return exchangeRates;
            }

            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                if (p.IsMaster) NotifyObservers(-1);
                throw new ArgumentException("day was not returned by GetAvailableDates()");
            }
            catch (Exception ex)
            {
                if (p.IsMaster) NotifyObservers(-1);
                throw new IOException("response unavailable or in unexpected format", ex);
            }
            finally
            {
                if (p.IsMaster) _lock.Release();
            }
        }

        /// internal version of GetExchangeRatesHistory; customizable lock, and progress start/stop value
        private async Task<IList<ExchangeRate>> GetExchangeRatesHistory2(
            Currency currency, DateTime startDay, DateTime stopDay, ProgressNotify p)
        {
            if (p.IsMaster) _lock.WaitOne();

            try
            {
                NotifyObservers(p.Start);

                var days = _dateToFilename
                    .Where(s => s.Key >= startDay && s.Key <= stopDay)
                    .Select(s => s.Key)
                    .OrderBy(day => day);

                var progressPerDay = (p.Stop + p.Start)/days.Count();
                var progress = p.Start;

                var result = new List<ExchangeRate>();
                foreach (var dateTime in days)
                {
                    var exchangeRates =
                        await GetExchangeRates2(dateTime, new ProgressNotify(progress, progress + progressPerDay));
                    var exchangeRate = exchangeRates.First(e => e.Currency.Equals(currency));
                    result.Add(exchangeRate);
                    progress += progressPerDay;
                    NotifyObservers(progress);
                }

                return result.AsReadOnly();
            }

            catch (Exception ex)
            {
                if (p.IsMaster) NotifyObservers(-1);
                throw new IOException("response unavailable or in unexpected format", ex);
            }
            finally
            {
                if (p.IsMaster) _lock.Release();
            }
        }

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

        private int CalculateProgress(ProgressNotify p, int percentDone)
        {
            return p.Start + (p.Stop - p.Start)*(percentDone/100);
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

        private class ProgressNotify
        {
            public static readonly ProgressNotify Master = new ProgressNotify(0, 100);

            public readonly bool IsMaster;
            public readonly int Start;
            public readonly int Stop;

            public ProgressNotify(int start, int stop)
            {
                IsMaster = (start == 0) && (stop == 100);
                Start = start;
                Stop = stop;
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