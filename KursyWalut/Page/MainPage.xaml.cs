using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using KursyWalut.Cache;
using KursyWalut.Helper;
using KursyWalut.Model;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KursyWalut.Page
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        private readonly ICache _cache;
        private readonly EventHandler<int> _toProgressNotifier;
        private object _historyPivotBackup;

        public MainPage()
        {
            Vm = new MainPageVm();
            InitializeComponent();

            _cache = CacheHelper.GetStandard();
            _toProgressNotifier = (sender, i) => Vm.Progress = i;

            _historyPivotBackup = MainPivot.Items?[1];
            MainPivot.Items?.RemoveAt(1);

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        public MainPageVm Vm { get; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AvgInit();
        }

        // ---------------------------------------------------------------------------------------------------------------

        private async void AvgInit()
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            Vm.ChangesEnabled = false;
            using (var h = new ProviderHelper(_cache, _toProgressNotifier))
            {
                var lastProgress = h.Progress.SubPercent(0.00, 0.10);
                var lastAvailableDay = await h.ErService.GetLastAvailableDay(lastProgress);
                Vm.AvgDate = lastAvailableDay;

                var erProgress = h.Progress.SubPercent(0.10, 0.50);
                Vm.AvgEr = await h.ErService.GetExchangeRates(lastAvailableDay, erProgress);

                var availProgress = h.Progress.SubPercent(0.50, 1.00);
                Vm.AvailDates = await h.ErService.GetAllAvailablesDay(availProgress);

                Vm.ChangesEnabled = true;
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine("{0}-{1} time: {2}", nameof(MainPage), nameof(AvgInit), sw.Elapsed.ToString("mm':'ss':'fff"));
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        private async void AvgReload(DateTime date)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            Vm.ChangesEnabled = false;
            using (var h = new ProviderHelper(_cache, _toProgressNotifier))
            {
                Vm.AvgEr = await h.ErService.GetExchangeRates(date, h.Progress);
                Vm.ChangesEnabled = true;
            }
#if DEBUG
            sw.Stop();
            Debug.WriteLine("{0}-{1} time: {2}", nameof(MainPage), nameof(AvgReload), sw.Elapsed.ToString("mm':'ss':'fff"));
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void AvgDatePicker_OnCalendarViewDayItemChanging(
            CalendarView sender,
            CalendarViewDayItemChangingEventArgs e)
        {
            if ((e.Item != null) && !Vm.AvailDates.Contains(e.Item.Date.DateTime.Date))
            {
                e.Item.IsBlackout = true;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void AvgDatePicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs e)
        {
            if ((e.NewDate != null) && Vm.AvgCalendarEnabled)
            {
                AvgReload(e.NewDate.Value.Date);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void AvgList_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var listView = sender as ListView;
            var selectedItem = listView?.SelectedItem as ExchangeRate;
            if (selectedItem == null) return;

            if (_historyPivotBackup != null)
            {
                MainPivot.Items?.Add(_historyPivotBackup);
                _historyPivotBackup = null;
            }

            Vm.HisCurrency = selectedItem.Currency;
            MainPivot.SelectedIndex = 1;
            listView.SelectedValue = null;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void OnBackRequested(object s, BackRequestedEventArgs e)
        {
            if (MainPivot.SelectedIndex > 0)
            {
                MainPivot.SelectedIndex = MainPivot.SelectedIndex - 1;
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }
    }
}