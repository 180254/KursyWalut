using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using KursyWalut.Model;

namespace KursyWalut.Page
{
    public class MainPageVm : ViewModelBase
    {
        private IList<ExchangeRate> _avgExchangeRates;
        private IList<DateTime> _availDates;
        private bool _avgListEnabled;
        private bool _calendarEnabled;
        private DateTimeOffset? _avgDate;
        private Currency _currency;
        private int _progress;

        public IList<ExchangeRate> AvgExchangeRates
        {
            get { return _avgExchangeRates; }
            set { Set(() => AvgExchangeRates, ref _avgExchangeRates, value); }
        }

        public IList<DateTime> AvailDates
        {
            get { return _availDates; }
            set { Set(() => AvailDates, ref _availDates, value); }
        }

        public bool AvgListEnabled
        {
            get { return _avgListEnabled; }
            set { Set(() => AvgListEnabled, ref _avgListEnabled, value); }
        }

        public bool CalendarEnabled
        {
            get { return _calendarEnabled; }
            set { Set(() => CalendarEnabled, ref _calendarEnabled, value); }
        }

        public DateTimeOffset? AvgDate
        {
            get { return _avgDate; }
            set { Set(() => AvgDate, ref _avgDate, value); }
        }

        public Currency Currency
        {
            get { return _currency; }
            set { Set(() => Currency, ref _currency, value); }
        }

        public int Progress
        {
            get { return _progress; }
            set { Set(() => Progress, ref _progress, value); }
        }

        public bool ChangesEnabled
        {
            set
            {
                AvgListEnabled = value;
                CalendarEnabled = value;
            }
        }
    }
}