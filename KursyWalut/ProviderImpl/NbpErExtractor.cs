using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using KursyWalut.Model;

namespace KursyWalut.ProviderImpl
{
    public class NbpErExtractor : IDisposable
    {
        private readonly HttpClient _client = new HttpClient();

        public void Dispose()
        {
            _client.Dispose();
        }

        /// <exception cref="T:System.Exception">If something go wrong.</exception>
        public async Task<string> GetHttpResponse(string requestUri, Encoding encoding)
        {
            var bytes = await _client.GetByteArrayAsync(requestUri);
            var str = encoding.GetString(bytes);
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        /// <exception cref="T:System.FormatException">response is in invalid format.</exception>
        public IEnumerable<string> ParseFilenames(string response)
        {
            var filenames = response.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            if (filenames.Length == 0)
                throw new FormatException("empty response(pf)");

            return filenames;
        }

        // ---------------------------------------------------------------------------------------------------------------

        /// <exception cref="T:System.FormatException">filename is in invalid format.</exception>
        public DateTimeOffset ParseDateTimeOffset(string filename)
        {
            try
            {
                return DateTimeOffset.ParseExact(filename.Substring(5, 6), "yyMMdd", CultureInfo.InvariantCulture);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new FormatException("filename is too short(pdt); filename = " + filename, ex);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        /// <exception cref="T:System.FormatException">xContainer is in invalid format.</exception>
        public IList<ExchangeRate> ParseExchangeRates(XContainer xC, DateTimeOffset day)
        {
            var exchangeRates = xC
                .Descendants("tabela_kursow")
                .Descendants("pozycja")
                .Select(xmlEr => ParseExchangeRate(xmlEr, day))
                .ToImmutableList();

            if (exchangeRates.Count == 0)
                throw new FormatException("empty response(per); xC = " + xC);

            return exchangeRates;
        }

        // ---------------------------------------------------------------------------------------------------------------

        /// <exception cref="T:System.FormatException">xContainer is in invalid format.</exception>
        public ExchangeRate ParseExchangeRate(XContainer xC, DateTimeOffset day)
        {
            try
            {
                var avarageRate = xC.Element("kurs_sredni").Value.Replace(',', '.');

                return new ExchangeRate(
                    day,
                    ParseCurrency(xC),
                    double.Parse(avarageRate, CultureInfo.InvariantCulture));
            }

            catch (Exception ex)
                when (ex is NullReferenceException || ex is OverflowException || ex is ArgumentException)
            {
                throw new FormatException("xContainer is in invalid format(per); xC = " + xC);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        /// <exception cref="T:System.FormatException">xContainer is in invalid format.</exception>
        public Currency ParseCurrency(XContainer xC)
        {
            try
            {
                return new Currency(
                    xC.Element("kod_waluty").Value,
                    (xC.Element("nazwa_waluty") ?? xC.Element("nazwa_kraju")).Value,
                    int.Parse(xC.Element("przelicznik").Value));
            }

            catch (Exception ex) when (ex is NullReferenceException || ex is OverflowException)
            {
                throw new FormatException("xContainer is in invalid format(pc); xC = " + xC);
            }
        }
    }
}