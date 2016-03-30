using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Bezysoftware.Navigation.BackButton;
using GalaSoft.MvvmLight.Ioc;
using KursyWalut.Helper;
using KursyWalut.Model;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KursyWalut.Page
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        private readonly MainVm _mainPageVm;
        private readonly EventHandler<int> _toProgressNotifier;

        public MainPage()
        {
            InitializeComponent();
            _mainPageVm = SimpleIoc.Default.GetInstance<MainVm>();
            _toProgressNotifier = (sender, i) => _mainPageVm.Progress = i;
            NavigationCacheMode = NavigationCacheMode.Enabled;
//            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility.
           Init();
        }

        private async void Init()
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            using (var h = new ProviderHelper(_toProgressNotifier))
            {
                if (_mainPageVm.Date == null) { 
                var lastAvailableDay =
                    await h.ErService.GetLastAvailableDay(h.Progress.SubPercent(0.00, 0.10));
                _mainPageVm.Date = lastAvailableDay;

                // ReSharper disable once PossibleInvalidOperationException
                    _mainPageVm.ExchangeRates =
                    await
                        h.ErService.GetExchangeRates(_mainPageVm.Date.Value.Date, h.Progress.SubPercent(0.10, 0.50));
                    }
                _mainPageVm.AvailDates =
                    await h.ErService.GetAllAvailablesDay(h.Progress.SubPercent(0.50, 1.00));
                _mainPageVm.CalendarEnabled = true;
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
                _mainPageVm.ExchangeRates =
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
            if ((e.Item != null) && !_mainPageVm.AvailDates.Contains(e.Item.Date.DateTime.Date))
            {
                e.Item.IsBlackout = true;
            }
        }

        private void CalendarDatePicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs args)
        {
            if ((args.NewDate != null) && (_mainPageVm != null) && _mainPageVm.CalendarEnabled)
            {
                Reload(args.NewDate.Value.Date);
            }
        }

        private void ListView_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var listView = sender as ListView;
            var selectedItem = listView?.SelectedItem as ExchangeRate;

            if (selectedItem != null)
            {
                Frame.Navigate(typeof (HistoryPage), selectedItem.Currency);
                listView.SelectedValue = null;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame rootFrame = Window.Current.Content as Frame;
            Debug.WriteLine("ON NAVI TO" + e.Parameter);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
//            e.Parameter = 1;
            Debug.WriteLine("ON NAVI FROM" + e.Parameter);

        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
//            e.NavigationTransitionInfo.P
//            e.Parameter = 1;
        }


    }
}