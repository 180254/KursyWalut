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
    public class NbpErProvider : IErProvider, ICacheable
    {
        private readonly Encoding _utf8;
        private readonly Encoding _iso88592;

        // ---------------------------------------------------------------------------------------------------------------

        private readonly NbpErExtractor _extractor;

        private readonly ICache _cache;
        private IDictionary<DateTimeOffset, string> _dayToFilename;
        private bool _dayToFilenameChanged;

        // ---------------------------------------------------------------------------------------------------------------

        public NbpErProvider(ICache cache)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _utf8 = new UTF8Encoding(false);
            _iso88592 = Encoding.GetEncoding("ISO-8859-2");

            _extractor = new NbpErExtractor();
            _cache = cache;
        }

        public async Task InitCache(IPProgress p)
        {
            _dayToFilename =
                await _cache.Get<IDictionary<DateTimeOffset, string>>(nameof(_dayToFilename))
                ?? new Dictionary<DateTimeOffset, string>();

            p.ReportProgress(1.00);
        }

        public async Task FlushCache(IPProgress p)
        {
            if (_dayToFilenameChanged)
                await _cache.Store(nameof(_dayToFilename), _dayToFilename);

            _dayToFilenameChanged = false;
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

        public async Task<IList<DateTimeOffset>> GetAvailableDays(int year, IPProgress p)
        {
            try
            {
                var availYears = await GetAvailableYears(p.SubPercent(0.00, 0.15));
                var first = availYears.First(y => y.Equals(year)); // possible InvalidOperationException

                var filenames = await DownloadFilenamesForYear(year);
                p.ReportProgress(0.70);

                var avgFilenames = filenames.Where(f => f.StartsWith("a", StringComparison.Ordinal));
                var result = ParseAndCacheDates(avgFilenames);
                p.ReportProgress(1.00);

                return result;
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


        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTimeOffset day, IPProgress p)
        {
            var response = "?";
            try
            {
                var filename = _dayToFilename[day]; // possible KeyNotFoundException
                var url = string.Format("http://www.nbp.pl/kursy/xml/{0}.xml", filename);
                response = await _extractor.GetHttpResponse(url, _iso88592);
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

        private async Task<IEnumerable<string>> DownloadFilenamesForYear(int year)
        {
            var urlYear = year == DateTimeOffset.Now.Year ? "" : year.ToString();
            var url = "http://www.nbp.pl/kursy/xml/dir" + urlYear + ".txt";

            var dirResponse = await _extractor.GetHttpResponse(url, _utf8);
            return _extractor.ParseFilenames(dirResponse);
        }

        private IList<DateTimeOffset> ParseAndCacheDates(IEnumerable<string> filenames)
        {
            _dayToFilenameChanged = true;

            var result = new List<DateTimeOffset>();
            foreach (var filename in filenames)
            {
                var day = _extractor.ParseDateTimeOffset(filename);
                _dayToFilename.Add(day, filename);
                result.Add(day);
            }

            return result.AsReadOnly();
        }
    }
}