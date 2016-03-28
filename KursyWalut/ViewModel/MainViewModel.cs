using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using KursyWalut.Model;
using KursyWalut.Progress;

namespace KursyWalut.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IList<DateTime> _availDates;
        private IList<ExchangeRate> _exchangeRates;
        private Integer _progressValue = new Integer(0);

        public IList<DateTime> AvailDates
        {
            get { return _availDates; }
            set { Set(() => AvailDates, ref _availDates, value); }
        }

        public IList<ExchangeRate> ExchangeRates
        {
            get { return _exchangeRates; }
            set { Set(() => ExchangeRates, ref _exchangeRates, value); }
        }

        public Integer ProgressValue
        {
            get { return _progressValue; }
            set { Set(() => ProgressValue, ref _progressValue, value); }
        }
    }
}