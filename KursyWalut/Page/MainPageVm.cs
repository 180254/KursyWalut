using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
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
        private Visibility _avgCalendarVisible = Visibility.Visible;
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

        private IList<Brush> _pivotForegrounds = new List<Brush>
        {
            GetSolidColorBrush("#FF6C4A15"),
            GetSolidColorBrush("#FFD1B280")
        };

        private bool _bottomAppBarIsOpen;
        private int _progress;
        private int _progressMax = 10000;

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

        public IList<Brush> PivotForegrounds
        {
            get { return _pivotForegrounds; }
            set { Set(() => PivotForegrounds, ref _pivotForegrounds, value); }
        }

        public bool BottomAppBarIsOpen
        {
            get { return _bottomAppBarIsOpen; }
            set { Set(() => BottomAppBarIsOpen, ref _bottomAppBarIsOpen, value); }
        }

        public int Progress
        {
            get { return _progress; }
            set { Set(() => Progress, ref _progress, value); }
        }

        public int ProgressMax
        {
            get { return _progressMax; }
            set { Set(() => ProgressMax, ref _progressMax, value); }
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

        public void RotatePivotForegrounds()
        {
            PivotForegrounds = Rotate(PivotForegrounds);
        }

        // ---------------------------------------------------------------------------------------------------------------

        public void HisDatesBackup()
        {
            _dateBackups[1] = HisDateFrom;
            _dateBackups[2] = HisDateTo;
        }

        public void HisDatesRecover()
        {
            HisDateFrom = _dateBackups[1];
            HisDateTo = _dateBackups[2];
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

        // ---------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///     "Converting Hex to Color in C# for Universal Windows Platform (UWP)"<br />
        ///     Credits: 07 March 2016 Joel Joseph www.joeljoseph.ne<br />
        ///     http://www.joeljoseph.net/converting-hex-to-color-in-universal-windows-platform-uwp/
        /// </summary>
        private static Brush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            var a = (byte) Convert.ToUInt32(hex.Substring(0, 2), 16);
            var r = (byte) Convert.ToUInt32(hex.Substring(2, 2), 16);
            var g = (byte) Convert.ToUInt32(hex.Substring(4, 2), 16);
            var b = (byte) Convert.ToUInt32(hex.Substring(6, 2), 16);
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        private static List<T> Rotate<T>(IEnumerable<T> source)
        {
            var copy = source.ToList();

            var first = copy[0];
            copy.RemoveAt(0);
            copy.Add(first);

            return copy;
        }
    }
}