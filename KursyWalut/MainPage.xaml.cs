using System;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight.Ioc;
using KursyWalut.Helper;
using KursyWalut.ViewModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KursyWalut
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MainViewModel _mainViewModel;
        private readonly EventHandler<int> _toProgressNotifier;

        public MainPage()
        {
            InitializeComponent();
            _mainViewModel = SimpleIoc.Default.GetInstance<MainViewModel>();
            _toProgressNotifier = (sender, i) => _mainViewModel.Progress = i;

            Init();
        }

        private async void Init()
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            using (var h = new ProviderHelper(_toProgressNotifier))
            {
                var lastAvailableDay =
                    await h.ErService.GetLastAvailableDay(h.Progress.SubPercent(0.00, 0.10));
                _mainViewModel.Date = lastAvailableDay;

                _mainViewModel.ExchangeRates =
                    await h.ErService.GetExchangeRates(lastAvailableDay, h.Progress.SubPercent(0.10, 0.50));

                _mainViewModel.AvailDates =
                    await h.ErService.GetAllAvailablesDay(h.Progress.SubPercent(0.50, 1.00));
                _mainViewModel.CalendarEnabled = true;
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine("{0} time: {1}", nameof(Init), sw.Elapsed.ToString("mm':'ss':'fff"));
#endif
        }


        private async void Reload(DateTime date)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            using (var h = new ProviderHelper(_toProgressNotifier))
            {
                _mainViewModel.ExchangeRates =
                    await h.ErService.GetExchangeRates(date, h.Progress.SubPercent(0.00, 1.00));
            }
#if DEBUG
            sw.Stop();
            Debug.WriteLine("{0} time: {1}", nameof(Reload), sw.Elapsed.ToString("mm':'ss':'fff"));
#endif
        }

        private void CalendarDatePicker_OnCalendarViewDayItemChanging(
            CalendarView sender,
            CalendarViewDayItemChangingEventArgs e)
        {
            if ((e.Item != null) && !_mainViewModel.AvailDates.Contains(e.Item.Date.DateTime.Date))
            {
                e.Item.IsBlackout = true;
            }
        }

        private void CalendarDatePicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs args)
        {
            if ((args.NewDate != null) && (_mainViewModel != null) && _mainViewModel.CalendarEnabled)
            {
                Reload(args.NewDate.Value.Date);
            }
        }
    }
}