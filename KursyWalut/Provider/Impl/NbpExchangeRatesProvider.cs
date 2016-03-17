using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using KursyWalut.Cache;

namespace KursyWalut.Provider.Impl
{
    internal class NbpExchangeRatesProvider : IExchangeRatesProvider
    {
        private readonly Encoding _utf8;
        private readonly Encoding _iso88592;

        // -----------------------------------------------------------------------------------------

        private readonly IList<IObserver<int>> _observers;
        private readonly IDictionary<DateTime, string> _dayToFilename;

        // -----------------------------------------------------------------------------------------

        private readonly NbpExchangeRateExtractor _extractor;

        // -----------------------------------------------------------------------------------------

        public NbpExchangeRatesProvider(ICache cache)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _utf8 = Encoding.UTF8;
            _iso88592 = Encoding.GetEncoding("ISO-8859-2");

            _observers = new List<IObserver<int>>();
            _dayToFilename = cache.SetIfAbsent("_dayToFilename", new Dictionary<DateTime, string>());

            _extractor = new NbpExchangeRateExtractor();
        }

        // -----------------------------------------------------------------------------------------

        public IDisposable Subscribe(IObserver<int> observer)
        {
            _observers.Add(observer);
            return new ObservationDispose<int>(_observers, observer);
        }

        // -----------------------------------------------------------------------------------------

        public async Task<IList<int>> GetAvailableYears(Progress p)
        {
            NotifyObservers(p, 0.00);
            const int startYear = 2002;
            var endYear = DateTime.Now.Year;
            var years = Enumerable.Range(startYear, endYear - startYear + 1).ToImmutableList();

            NotifyObservers(p, 1.00);
            return await Task.Run(() => years);
        }

        public async Task<IList<DateTime>> GetAvailableDays(int year, Progress p)
        {
            try
            {
                NotifyObservers(p, 0.00);
                var availYears = await GetAvailableYears(p.PartialPercent(0.00, 0.15));
                var first = availYears.First(y => y.Equals(year)); // possible InvalidOperationException

                var year2 = year == DateTime.Now.Year ? "" : year.ToString();
                var dir = await GetHttpResponse("http://www.nbp.pl/kursy/xml/dir" + year2 + ".txt", _utf8);
                var filenames = _extractor.ParseFilenames(dir);

                NotifyObservers(p, 0.70);
                var result = new List<DateTime>();
                foreach (var filename in filenames.Where(f => f.StartsWith("a")))
                {
                    var day = _extractor.ParseDateTime(filename);
                    _dayToFilename.Add(day, filename);
                    result.Add(day);
                }

                NotifyObservers(p, 1.00);
                return result.AsReadOnly();
            }

            catch (Exception ex) when (ex is InvalidOperationException)
            {
                NotifyObservers(p, -1.00);
                throw new ArgumentException("year was not returned by GetAvailableYears()");
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is IOException))
            {
                NotifyObservers(p, -1.00);
                throw new IOException("response unavailable or in unexpected format(0)", ex);
            }
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day, Progress p)
        {
            try
            {
                NotifyObservers(p, 0.00);
                var filename = _dayToFilename[day]; // possible KeyNotFoundException
                var response = await GetHttpResponse("http://www.nbp.pl/kursy/xml/" + filename + ".xml", _iso88592);

                NotifyObservers(p, 0.60);
                var xml = XDocument.Load(new StringReader(response));

                NotifyObservers(p, 0.70);
                var exchangeRates = _extractor.ParseExchangeRates(xml, day);

                NotifyObservers(p, 1.00);
                return exchangeRates;
            }

            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                throw new ArgumentException("day was not returned by GetAvailableDays(day.year)");
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is IOException))
            {
                throw new IOException("response unavailable or in unexpected format(1)", ex);
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

        // -----------------------------------------------------------------------------------------

        public static bool AlmostEqual(double x, double y)
        {
            var epsilon = Math.Max(Math.Abs(x), Math.Abs(y))*1E-5;
            return Math.Abs(x - y) <= epsilon;
        }

        private void NotifyObservers(Progress p, double percentDone)
        {
            var error = AlmostEqual(percentDone, -1.00);
            var next = p.ComputePercent(percentDone);

            foreach (var observer in _observers)
            {
                if (error)
                    observer.OnError(null);
                else
                    observer.OnNext(next);
            }
        }
    }
}