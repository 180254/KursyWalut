using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using KursyWalut.Model;

namespace KursyWalut.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IList<ExchangeRate> _exchangeRates;
        private IList<DateTime> _availDates;
        private bool _calendarEnabled;
        private DateTimeOffset? _date;
        private int _progress;

        public IList<ExchangeRate> ExchangeRates
        {
            get { return _exchangeRates; }
            set { Set(() => ExchangeRates, ref _exchangeRates, value); }
        }

        public IList<DateTime> AvailDates
        {
            get { return _availDates; }
            set { Set(() => AvailDates, ref _availDates, value); }
        }

        public bool CalendarEnabled
        {
            get { return _calendarEnabled; }
            set { Set(() => CalendarEnabled, ref _calendarEnabled, value); }
        }

        public DateTimeOffset? Date
        {
            get { return _date; }
            set { Set(() => Date, ref _date, value); }
        }

        public int Progress
        {
            get { return _progress; }
            set { Set(() => Progress, ref _progress, value); }
        }
    }
}