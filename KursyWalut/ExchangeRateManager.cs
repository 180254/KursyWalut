using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using KursyWalut.Model;

namespace KursyWalut
{
    class ExchangeRateManager
    {
        private ObservableCollection<ExchangeRate> exchangeRates { get; set; }
        private string xmlUrl = "http://www.nbp.pl/kursy/xml/a002z160105.xml"; //test, bo poki co stala wartosc wpisana na sztywno.

        private HttpClient httpClient;
        private Encoding encoding;


        public ExchangeRateManager()
        {
            exchangeRates = new ObservableCollection<ExchangeRate>();

            httpClient = new HttpClient();

            initProperEncoding();
        }

        private void initProperEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding("ISO-8859-2");
        }

        private async Task<string> loadProperXml()
        {
            var bytes = await httpClient.GetByteArrayAsync(xmlUrl);
            return encoding.GetString(bytes);
        }

        public ObservableCollection<ExchangeRate> loadCurrentExchangeRates()
        {
            Task<string> xmlLoadTask = loadProperXml();
            xmlLoadTask.Wait();

            string loadedXml = xmlLoadTask.Result;
            XDocument xDocument = XDocument.Parse(loadedXml);
            foreach (XElement element in xDocument.Descendants("pozycja"))
            {
//                exchangeRates.Add(new ExchangeRate(element));
            }

            //Debug.WriteLine("Loaded xml: " + loadedXml);

            //exchangeRates.Add(new ExchangeRate()); //test, do wywalenia...
            //exchangeRates.Add(new ExchangeRate()); //test, do wywalenia...
            //exchangeRates.Add(new ExchangeRate()); //test, do wywalenia...
            Debug.WriteLine(loadedXml);
            return exchangeRates;
        }
    }
}
