using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KursyWalut.Cache;
using KursyWalut.Model;
using KursyWalut.Progress;
using KursyWalut.ProviderImpl;

namespace KursyWalut.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IList<DateTime> _availDates = new List<DateTime>();
        private IList<ExchangeRate> _exchangeRates = new List<ExchangeRate>();
        private Integer _progressValue = 0;

        public MainViewModel()
        {
       Init();;
        }

        public IList<DateTime> AvailDates
        {
            get { return _availDates; }
            private set { Set(() => AvailDates, ref _availDates, value); }
        }

        public IList<ExchangeRate> ExchangeRates
        {
            get { return _exchangeRates; }
            private set { Set(() => ExchangeRates, ref _exchangeRates, value); }
        }

        public Integer ProgressValue
        {
            get { return _progressValue; }
            private set { Set(() => ProgressValue, ref _progressValue, value); }
        }



        public async void Init()
        {
            var cache = LocalStorageCache.GetStandard();
            var progress = PProgress.NewMaster();
            progress.ProgressChanged += (sender, i) => _progressValue.Set(i);

            var nbpProvider = new NbpExchangeRatesProvider(cache, progress.SubPercent(0.00, 0.05));
            var cacheProvider = new CacheExchangeRateProvider(nbpProvider, cache, progress.SubPercent(0.05, 0.10));
            var erService = new StandardExchangeRateService(cacheProvider);

            var lastAvailableDay = await erService.GetLastAvailableDay(progress.SubPercent(0.10, 0.15));
            var exchangeRates = await erService.GetExchangeRates(lastAvailableDay, progress.SubPercent(0.15, 0.50));

            AvailDates = await erService.GetAllAvailablesDay(progress.SubPercent(0.50, 1.00));
            erService.FlushCache(PProgress.NewMaster());

            ExchangeRates = exchangeRates;
;
//            _exchangeRates.Concat(exchangeRates);
//            foreach (var exchangeRate in exchangeRates)
//            {
//                _exchangeRates.Add(exchangeRate);
//            }
            progress.ReportProgress(1.00);
        }



        private void CalendarDatePicker_OnCalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs e)
        {

        }

        public async void Reload(DateTime newDate)
        {
            var cache = LocalStorageCache.GetStandard();
            var progress = PProgress.NewMaster();
            progress.ProgressChanged += (sender, i) => _progressValue.Set(i);
            progress.ReportProgress(0.00);
            var nbpProvider = new NbpExchangeRatesProvider(cache, progress.SubPercent(0.00, 0.05));
            var cacheProvider = new CacheExchangeRateProvider(nbpProvider, cache, progress.SubPercent(0.05, 0.10));
            var erService = new StandardExchangeRateService(cacheProvider);


            ExchangeRates = await erService.GetExchangeRates(newDate, progress.SubPercent(0.10, 1.00));
            progress.ReportProgress(1.00);


        }
    }
}