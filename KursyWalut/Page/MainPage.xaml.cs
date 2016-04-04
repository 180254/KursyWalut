using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using KursyWalut.Helper;
using KursyWalut.Model;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KursyWalut.Page
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly ResourceLoader _res;
        private readonly EventHandler<int> _progressSubscriber;
        private readonly ProviderHelper _providerHelper;
        private object _historyPivotBackup;
        private bool _historyDrawn;

        public MainPage()
        {
            Vm = new MainPageVm();
            InitializeComponent();

            // initialize provider helper
            var cache = CacheHelper.GetStandardLsc();
            _progressSubscriber = (sender, i) => Vm.Progress = i;
            _providerHelper = new ProviderHelper(cache, _progressSubscriber);

            // remove "history" pivot
            _historyPivotBackup = MainPivot.Items?[1];
            MainPivot.Items?.RemoveAt(1);

            _res = ResourceLoader.GetForCurrentView();
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            Application.Current.UnhandledException += OnUnhandledException;
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

            using (var h = _providerHelper.Helper())
            {
                await h.InitCache();

                var firstProgress = h.Progress.SubPercent(0.00, 0.05);
                var firstAvailableDay = await h.ErService.GetFirstAvailableDay(firstProgress);
                Vm.HisDateFromMin = firstAvailableDay;

                var lastProgress = h.Progress.SubPercent(0.05, 0.10);
                var lastAvailableDay = await h.ErService.GetLastAvailableDay(lastProgress);
                Vm.AvgDate = lastAvailableDay;
                Vm.HisDateFrom = lastAvailableDay.AddYears(-1);
                Vm.HisDateTo = lastAvailableDay;
                Vm.HisDateToMax = lastAvailableDay;

                var erProgress = h.Progress.SubPercent(0.10, 0.50);
                Vm.AvgErList = await h.ErService.GetExchangeRates(lastAvailableDay, erProgress);

                var availProgress = h.Progress.SubPercent(0.50, 1.00);
                Vm.AvailDates = await h.ErService.GetAllAvailablesDay(availProgress);

                await h.FlushCache();
            }

            Vm.InitDone = true;
            Vm.ChangesEnabled = true;
            Vm.AllDatesBackup();

#if DEBUG
            DebugElapsedTime(sw, nameof(AvgInit));
#endif
        }


        // ---------------------------------------------------------------------------------------------------------------

        private async void AvgReload(DateTimeOffset date)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            Vm.ChangesEnabled = false;

            using (var h = _providerHelper.Helper())
            {
                await h.InitCache();
                Vm.AvgErList = await h.ErService.GetExchangeRates(date, h.Progress);
                await h.FlushCache();
            }

            Vm.ChangesEnabled = true;
            Vm.AllDatesBackup();

#if DEBUG
            DebugElapsedTime(sw, nameof(AvgReload));
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void AvgDatePicker_OnCalendarViewDayItemChanging(
            CalendarView sender,
            CalendarViewDayItemChangingEventArgs e)
        {
            if ((e.Item != null) && !Vm.AvailDates.Contains(e.Item.Date.Date))
            {
                e.Item.IsBlackout = true;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void AvgDatePicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs e)
        {
            // ReSharper disable once InvertIf
            if ((e.NewDate != null) && Vm.AvgActionEnabled)
            {
                Vm.AvgDate = e.NewDate.Value.Date;

                if (e.NewDate?.Date != e.OldDate?.Date)
                {
                    AvgReload(e.NewDate.Value.Date);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void AvgList_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedItem = AvgList.SelectedItem as ExchangeRate;
            if (selectedItem == null) return;

            var newCurrencySelected =
                !Equals(Vm.HisCurrency, selectedItem.Currency)
                && (_historyPivotBackup == null);

            if (newCurrencySelected)
            {
                _historyDrawn = false;
                Vm.HisErList = null;

                // remove whole chart, and be empty space again - render hack
                var series = HisChart.Series[0];
                (series as LineSeries)?.Points?.Clear();
                HisChart.Series.Remove(series);
                HisChart.Series.Add(series);
            }

            // restore history pivot
            if (_historyPivotBackup != null)
            {
                MainPivot.Items?.Add(_historyPivotBackup);
                _historyPivotBackup = null;
            }

            Vm.HisCurrency = selectedItem.Currency;
            MainPivot.SelectedIndex = 1; // change pivot to history
            AvgList.SelectedValue = null;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private async void HisDraw_OnClick(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            if ((Vm.HisDateFrom == null) || (Vm.HisDateTo == null))
            {
                return;
            }

            Vm.ChangesEnabled = false;

            using (var h = _providerHelper.Helper())
            {
                await h.InitCache();

                var ers = new List<ExchangeRate>();
                var expectedSize = (int) (HisChart.ActualWidth*1.05);
                await h.ErService.GetExchangeRateAveragedHistory(
                    Vm.HisCurrency, Vm.HisDateFrom.Value, Vm.HisDateTo.Value,
                    ers, expectedSize, h.Progress);
                Vm.HisErList = ers;

                Debug.WriteLine("{0}-width={1};expectedSize={2};gotSize={3}",
                    nameof(HisDraw_OnClick), (int) HisChart.ActualWidth, expectedSize, ers.Count);

                await h.FlushCache();
            }

            _historyDrawn = true;
            Vm.ChangesEnabled = true;
            Vm.HisSaveEnabled = true;
            Vm.AllDatesBackup();

#if DEBUG
            DebugElapsedTime(sw, nameof(HisDraw_OnClick));
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void HisDateFromPicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs e)
        {
            if (e.NewDate != null)
            {
                Vm.HisDateFrom = e.NewDate.Value.Date;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void HitDateToPicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs e)
        {
            if (e.NewDate != null)
            {
                Vm.HisDateTo = e.NewDate.Value.Date;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private async void HisSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var suggestedName = string.Format("{0}_{1}-{2}",
                Vm.HisCurrency.Code,
                Vm.HisDateFrom?.ToString("ddMMyy") ?? "?",
                Vm.HisDateTo?.ToString("ddMMyy") ?? "?");

            Vm.ChangesEnabled = false;

            using (var h = new UiElementToPngHelper(HisChart, suggestedName, _progressSubscriber))
            {
                var saved = await h.Execute();

                if (saved)
                {
                    Vm.BottomAppBarIsOpen = false;
                    await new MessageDialog(_res.GetString("HisSaveConfirmation/Text")).ShowAsync();
                }
            }

            Vm.ChangesEnabled = true;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var historyPivotSelected = MainPivot.SelectedIndex == 1;
            Vm.HisSaveEnabled = historyPivotSelected && _historyDrawn;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void OnBackRequested(object s, BackRequestedEventArgs e)
        {
            // don't handle avg pivot - default action (close)
            e.Handled = MainPivot.SelectedIndex > 0;

            // ReSharper disable once InvertIf
            if (e.Handled)
            {
                // change pivot from history to avg
                MainPivot.SelectedIndex = MainPivot.SelectedIndex - 1;
                Vm.HisDatesRecover();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Vm.Progress = 0;
            await new MessageDialog(_res.GetString("NoInternet/Text")).ShowAsync();

            AvgList.SelectedItem = null;

            if (!Vm.InitDoneSet)
            {
                Vm.InitDone = false;
            }
            else
            {
                Vm.ChangesEnabled = true;
                Vm.AllDatesRecover();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void AvgRetryInitButton_OnClick(object sender, RoutedEventArgs e)
        {
            AvgInit();
        }

        // ---------------------------------------------------------------------------------------------------------------

        private static void DebugElapsedTime(Stopwatch sw, string methodName)
        {
            sw.Stop();
            Debug.WriteLine("{0}-time: {1}", methodName, sw.Elapsed.ToString("mm':'ss':'fff"));
        }
    }
}