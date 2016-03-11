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
        private readonly Semaphore _lock = new Semaphore(1, 1);

        private readonly IList<IObserver<int>> _observers = new List<IObserver<int>>();
        private readonly IDictionary<DateTime, string> _dateToFilename = new Dictionary<DateTime, string>();

        public NbpExchangeRatesProvider()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public async Task<IList<DateTime>> GetAvailableDates()
        {
            _lock.WaitOne();
            NotifyObservers(0);
            var result = new List<DateTime>();

            string[] filenames;
            using (var client = new HttpClient())
            {
                var dirResult = await client.GetStringAsync("http://www.nbp.pl/kursy/xml/dir.txt");
                filenames = dirResult.Split('\n');
            }

            NotifyObservers(70);
            foreach (var filename in filenames)
            {
                if (!filename.StartsWith("a"))
                    continue;

                var day = FilenameToDateTime(filename);
                _dateToFilename.Add(day, filename);
                result.Add(day);
            }

            NotifyObservers(100);
            _lock.Release();
            return result.AsReadOnly();
        }


        public async Task<IList<ExchangeRate>> GetExchangeRates(DateTime day)
        {
            _lock.WaitOne();
            NotifyObservers(0);

            if (!_dateToFilename.ContainsKey(day))
                throw new ArgumentException();
            var filename = _dateToFilename[day];

            XDocument xmlDoc;
            using (var client = new HttpClient())
            {
                var xmlBytes = await client.GetByteArrayAsync("http://www.nbp.pl/kursy/xml/" + filename + ".xml");
                var xmlString = Encoding.GetEncoding("ISO-8859-2").GetString(xmlBytes);
                xmlDoc = XDocument.Load(new StringReader(xmlString));
            }

            NotifyObservers(70);
            var exchangeRates = xmlDoc
                .Descendants("tabela_kursow")
                .Descendants("pozycja")
                .Select(x => XmlPositionToExchangeRate(day, x));

            NotifyObservers(100);
            _lock.Release();
            return exchangeRates.ToImmutableList();
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            _observers.Add(observer);
            return null;
        }

        private DateTime FilenameToDateTime(string fileName)
        {
            return DateTime.ParseExact(fileName.Substring(5, 6), "yyMMdd", CultureInfo.InvariantCulture);
        }

        private ExchangeRate XmlPositionToExchangeRate(DateTime day, XContainer x)
        {
            return new ExchangeRate(
                day,
                XmlPositionToCurrency(x),
                double.Parse(x.Element("kurs_sredni").Value.Replace(",", ".")));
        }

        private Currency XmlPositionToCurrency(XContainer x)
        {
            return new Currency(
                x.Element("kod_waluty").Value,
                x.Element("nazwa_waluty").Value,
                int.Parse(x.Element("przelicznik").Value));
        }

        private void NotifyObservers(int percentDone)
        {
            foreach (var observer in _observers)
            {
                if (percentDone == 100) observer.OnNext(percentDone);
                else observer.OnCompleted();
            }
        }
    }
}