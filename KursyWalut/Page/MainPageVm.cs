using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using KursyWalut.Model;

namespace KursyWalut.Page
{
    public class MainPageVm : ViewModelBase
    {
        private DateTimeOffset? _avgDate;
        private IList<ExchangeRate> _avgErList;
        private IList<DateTimeOffset> _availDates;
        private bool _avgActionEnabled;
        private Visibility _avgCalendarVisible = Visibility.Collapsed;
        private Visibility _avgRetryInitButtonVisible = Visibility.Collapsed;

        // ---------------------------------------------------------------------------------------------------------------

        private Currency _hisCurrency;
        private DateTimeOffset? _hisDateFrom;
        private DateTimeOffset? _hisDateTo;
        private readonly DateTimeOffset?[] _dateBackups = new DateTimeOffset?[3];
        private DateTimeOffset _hisDateFromMin;
        private DateTimeOffset _hisDateToMax;
        private IList<ExchangeRate> _hisErList;
        private bool _hisActionEnabled;
        private bool _hisSaveEnabled;


        // ---------------------------------------------------------------------------------------------------------------

        private bool _bottomAppBarIsOpen;
        private int _progress;

        // ---------------------------------------------------------------------------------------------------------------

        public DateTimeOffset? AvgDate
        {
            get { return _avgDate; }
            set { Set(() => AvgDate, ref _avgDate, value); }
        }

        public IList<ExchangeRate> AvgErList
        {
            get { return _avgErList; }
            set { Set(() => AvgErList, ref _avgErList, value); }
        }

        public IList<DateTimeOffset> AvailDates
        {
            get { return _availDates; }
            set { Set(() => AvailDates, ref _availDates, value); }
        }

        public bool AvgActionEnabled
        {
            get { return _avgActionEnabled; }
            set { Set(() => AvgActionEnabled, ref _avgActionEnabled, value); }
        }

        public Visibility AvgCalendarVisible
        {
            get { return _avgCalendarVisible; }
            set { Set(() => AvgCalendarVisible, ref _avgCalendarVisible, value); }
        }

        public Visibility AvgRetryInitButtonVisible
        {
            get { return _avgRetryInitButtonVisible; }
            set { Set(() => AvgRetryInitButtonVisible, ref _avgRetryInitButtonVisible, value); }
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

        public IList<ExchangeRate> HisErList
        {
            get { return _hisErList; }
            set { Set(() => HisErList, ref _hisErList, value); }
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

        public bool BottomAppBarIsOpen
        {
            get { return _bottomAppBarIsOpen; }
            set { Set(() => BottomAppBarIsOpen, ref _bottomAppBarIsOpen, value); }
        }

        public bool ChangesEnabled
        {
            set { AvgActionEnabled = HisActionEnabled = value; }
        }

        public bool InitDone
        {
            set
            {
                AvgCalendarVisible = value ? Visibility.Visible : Visibility.Collapsed;
                AvgRetryInitButtonVisible = !value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool InitDoneSet => !AvgCalendarVisible.Equals(AvgRetryInitButtonVisible);

        // ---------------------------------------------------------------------------------------------------------------

        public void HisDatesBackup()
        {
            _dateBackups[1] = HisDateFrom;
            _dateBackups[2] = HisDateTo;
        }

        public void HisDatesRecover()
        {
            _dateBackups[1] = HisDateFrom;
            _dateBackups[2] = HisDateTo;
        }

        public void AllDatesBackup()
        {
            _dateBackups[0] = AvgDate;
            HisDatesBackup();
        }

        public void AllDatesRecover()
        {
            AvgDate = _dateBackups[0];
            HisDatesRecover();
        }
    }
}