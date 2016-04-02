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
        private bool _avgActionEnabled;

        // ---------------------------------------------------------------------------------------------------------------

        private Currency _hisCurrency;
        private DateTimeOffset? _hisDateFrom;
        private DateTimeOffset? _hisDateTo;
        private readonly DateTimeOffset?[] _hisDateBackups = new DateTimeOffset?[2];
        private DateTimeOffset _hisDateFromMin;
        private DateTimeOffset _hisDateToMax;
        private IList<ExchangeRate> _hisEr;
        private bool _hisActionEnabled;
        private bool _hisSaveEnabled;


        // ---------------------------------------------------------------------------------------------------------------

        private int _progress;

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

        public bool AvgActionEnabled
        {
            get { return _avgActionEnabled; }
            set { Set(() => AvgActionEnabled, ref _avgActionEnabled, value); }
        }

        // ---------------------------------------------------------------------------------------------------------------

        public Currency HisCurrency
        {
            get { return _hisCurrency; }
            set { Set(() => HisCurrency, ref _hisCurrency, value); }
        }


        public DateTimeOffset? HisDateFrom
        {
            get { return _hisDateFrom; }
            set { Set(() => HisDateFrom, ref _hisDateFrom, value); }
        }

        public DateTimeOffset? HisDateTo
        {
            get { return _hisDateTo; }
            set { Set(() => HisDateTo, ref _hisDateTo, value); }
        }

        public DateTimeOffset HisDateFromMin
        {
            get { return _hisDateFromMin; }
            set { Set(() => HisDateFromMin, ref _hisDateFromMin, value); }
        }

        public DateTimeOffset HisDateToMax
        {
            get { return _hisDateToMax; }
            set { Set(() => HisDateToMax, ref _hisDateToMax, value); }
        }

        public IList<ExchangeRate> HisEr
        {
            get { return _hisEr; }
            set { Set(() => HisEr, ref _hisEr, value); }
        }

        public bool HisActionEnabled
        {
            get { return _hisActionEnabled; }
            set { Set(() => HisActionEnabled, ref _hisActionEnabled, value); }
        }

        public bool HisSaveEnabled
        {
            get { return _hisSaveEnabled; }
            set { Set(() => HisSaveEnabled, ref _hisSaveEnabled, value); }
        }


        // ---------------------------------------------------------------------------------------------------------------

        public int Progress
        {
            get { return _progress; }
            set { Set(() => Progress, ref _progress, value); }
        }

        public bool ChangesEnabled
        {
            set { AvgActionEnabled = HisActionEnabled = value; }
        }

        // ---------------------------------------------------------------------------------------------------------------

        public void HisDateBackup()
        {
            _hisDateBackups[0] = HisDateFrom;
            _hisDateBackups[1] = HisDateTo;
        }

        public void HisDateRecover()
        {
            HisDateFrom = _hisDateBackups[0];
            HisDateTo = _hisDateBackups[1];
        }
    }
}