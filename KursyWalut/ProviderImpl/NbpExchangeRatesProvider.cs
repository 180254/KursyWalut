using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using KursyWalut.Cache;
using KursyWalut.Model;
using KursyWalut.Progress;
using KursyWalut.Provider;

namespace KursyWalut.ProviderImpl
{
    public class NbpExchangeRatesProvider : IExchangeRatesProvider, ICacheable
    {
        private readonly Encoding _utf8;
        private readonly Encoding _iso88592;

        // ---------------------------------------------------------------------------------------------------------------

        private readonly NbpExchangeRateExtractor _extractor;

        private readonly ICache _cache;
        private readonly IDictionary<DateTime, string> _dayToFilename;

        // ---------------------------------------------------------------------------------------------------------------

        public NbpExchangeRatesProvider(ICache cache, IPProgress p)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _utf8 = new UTF8Encoding(false);
            _iso88592 = Encoding.GetEncoding("ISO-8859-2");

            _extractor = new NbpExchangeRateExtractor();

            _cache = cache;
            _dayToFilename = cache.Get<IDictionary<DateTime, string>>(
                nameof(_dayToFilename), () => new Dictionary<DateTime, string>());
            p.ReportProgress(1.00);
        }

        public void FlushCache(IPProgress p)
        {
            _cache.Store(nameof(_dayToFilename), _dayToFilename);
            p.ReportProgress(1.00);
        }

        // ---------------------------------------------------------------------------------------------------------------

        public async Task<IList<int>> GetAvailableYears(IPProgress p)
        {
            const int startYear = 2002;
            var endYear = DateTime.Now.Year;
            var years = Enumerable.Range(startYear, endYear - startYear + 1).ToImmutableList();

            p.ReportProgress(1.00);
            return await Task.Run(() => years);
        }

        public async Task<IList<DateTime>> GetAvailableDays(int year, IPProgress p)
        {
            try
            {
                var availYears = await GetAvailableYears(p.SubPercent(0.00, 0.15));
                // ReSharper disable once UnusedVariable
                var first = availYears.First(y => y.Equals(year)); // possible InvalidOperationException

                var year2 = year == DateTime.Now.Year ? "" : year.ToString();
                var dir = await _extractor.GetHttpResponse("http://www.nbp.pl/kursy/xml/dir" + year2 + ".txt", _utf8);
                var filenames = _extractor.ParseFilenames(dir);
                p.ReportProgress(0.70);

                var result = new List<DateTime>();
                foreach (var filename in filenames.Where(f => f.StartsWith("a", StringComparison.Ordinal)))
                {
                    var day = _extractor.ParseDateTime(filename);
                    _dayToFilename.Add(day, filename);
                    result.Add(day);
                }

                p.ReportProgress(1.00);
                return result.AsReadOnly();
            }

            catch (Exception ex) when (ex is InvalidOperationException)
            {
                p.ReportProgress(-1.00);
                throw new ArgumentException("year was not returned by GetAvailableYears()");
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is IOException))
            {
                p.ReportProgress(-1.00);
                throw new IOException("response unavailable or in unexpected format(0)", ex);
            }
        }

        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day, IPProgress p)
        {
            var response = "?";
            try
            {
                var filename = _dayToFilename[day]; // possible KeyNotFoundException
                response =
                    await _extractor.GetHttpResponse("http://www.nbp.pl/kursy/xml/" + filename + ".xml", _iso88592);

                p.ReportProgress(0.60);
                var xml = XDocument.Load(new StringReader(response));

                p.ReportProgress(0.70);
                var exchangeRates = _extractor.ParseExchangeRates(xml, day);

                p.ReportProgress(1.00);
                return exchangeRates;
            }

            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                throw new ArgumentException("day was not returned by GetAvailableDays(day.year)");
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is IOException))
            {
                throw new IOException("response unavailable or in unexpected format(1); response = " + response, ex);
            }
        }
    }
}