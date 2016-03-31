using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using KursyWalut.Model;

namespace KursyWalut.Page
{
    public class MainPageVm : ViewModelBase
    {
        private DateTimeOffset? _avgDate;
        private IList<ExchangeRate> _avgEr;
        private IList<DateTime> _availDates;
        private bool _avgListEnabled;
        private bool _avgCalendarEnabled;

        // ---------------------------------------------------------------------------------------------------------------

        private Currency _hisCurrency;
        private IList<ExchangeRate> _hisEr;

        // ---------------------------------------------------------------------------------------------------------------

        private int _progress;

        public MainPageVm()
        {
            HisEr = new List<ExchangeRate>();
            var random = new Random();
            for (var i = 0; i < 300; i++)
            {
                HisEr.Add(new ExchangeRate(DateTime.Now.AddDays(i), Currency.DummyForCode("USD"), random.Next(380, 450)/100.0));
            }

        }

        // ---------------------------------------------------------------------------------------------------------------

        public DateTimeOffset? AvgDate
        {
            get { return _avgDate; }
            set { Set(() => AvgDate, ref _avgDate, value); }
        }

        public IList<ExchangeRate> AvgEr
        {
            get { return _avgEr; }
            set { Set(() => AvgEr, ref _avgEr, value); }
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

        public bool AvgCalendarEnabled
        {
            get { return _avgCalendarEnabled; }
            set { Set(() => AvgCalendarEnabled, ref _avgCalendarEnabled, value); }
        }

        // ---------------------------------------------------------------------------------------------------------------

        public Currency HisCurrency
        {
            get { return _hisCurrency; }
            set { Set(() => HisCurrency, ref _hisCurrency, value); }
        }


        public IList<ExchangeRate> HisEr
        {
            get { return _hisEr; }
            set { Set(() => HisEr, ref _hisEr, value); }
        }

        // ---------------------------------------------------------------------------------------------------------------

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
                AvgCalendarEnabled = value;
            }
        }
    }
}