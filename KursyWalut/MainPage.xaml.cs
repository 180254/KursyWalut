using System;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight.Ioc;
using KursyWalut.ProviderImpl;
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
            _toProgressNotifier = (sender, i) => _mainViewModel.ProgressValue.Set(i);

            Init();
        }

        private async void Init()
        {
            using (var h = new ProviderHelper(_toProgressNotifier))
            {
                var lastAvailableDay =
                    await h.ErService.GetLastAvailableDay(h.Progress.SubPercent(0.00, 0.10));
                CalendarDatePicker.Date = lastAvailableDay;

                _mainViewModel.ExchangeRates =
                    await h.ErService.GetExchangeRates(lastAvailableDay, h.Progress.SubPercent(0.10, 0.50));

                _mainViewModel.AvailDates =
                    await h.ErService.GetAllAvailablesDay(h.Progress.SubPercent(0.50, 1.00));
                CalendarDatePicker.IsEnabled = true;
            }
        }


        private async void Reload(DateTime date)
        {
            using (var h = new ProviderHelper(_toProgressNotifier))
            {
                _mainViewModel.ExchangeRates =
                    await h.ErService.GetExchangeRates(date, h.Progress.SubPercent(0.00, 1.00));
            }
        }

        private void CalendarDatePicker_OnCalendarViewDayItemChanging(
            CalendarView sender,
            CalendarViewDayItemChangingEventArgs e)
        {
            if (e.Item != null && !_mainViewModel.AvailDates.Contains(e.Item.Date.DateTime.Date))
                e.Item.IsBlackout = true;
        }

        private void CalendarDatePicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate != null)
            {
                Reload(args.NewDate.Value.Date);
            }
        }
    }
}