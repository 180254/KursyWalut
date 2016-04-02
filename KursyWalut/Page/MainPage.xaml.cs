using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using KursyWalut.Helper;
using KursyWalut.Model;
using WinRTXamlToolkit.AwaitableUI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KursyWalut.Page
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly EventHandler<int> _progressSubscriber;
        private readonly ProviderHelper _providerHelper;
        private object _historyPivotBackup;
        private bool _historyDrawn;

        public MainPage()
        {
            Vm = new MainPageVm();
            InitializeComponent();

            var cache = CacheHelper.GetStandardLsc();
            _progressSubscriber = (sender, i) => Vm.Progress = i;
            _providerHelper = new ProviderHelper(cache, _progressSubscriber);

            // remove "history" pivot
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
                Vm.HisDateBackup();

                var erProgress = h.Progress.SubPercent(0.10, 0.50);
                Vm.AvgEr = await h.ErService.GetExchangeRates(lastAvailableDay, erProgress);

                var availProgress = h.Progress.SubPercent(0.50, 1.00);
                Vm.AvailDates = await h.ErService.GetAllAvailablesDay(availProgress);

                await h.FlushCache();
            }

            Vm.ChangesEnabled = true;

#if DEBUG
            DebugElapsedTime(sw, nameof(AvgInit));
#endif
        }


        // ---------------------------------------------------------------------------------------------------------------

        private async void AvgReload(DateTime date)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            Vm.ChangesEnabled = false;

            using (var h = _providerHelper.Helper())
            {
                await h.InitCache();
                Vm.AvgEr = await h.ErService.GetExchangeRates(date, h.Progress);
                await h.FlushCache();
            }

            Vm.ChangesEnabled = true;
#if DEBUG
            DebugElapsedTime(sw, nameof(AvgReload));
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
            // ReSharper disable once InvertIf
            if ((e.NewDate != null) && Vm.AvgActionEnabled)
            {
                AvgReload(e.NewDate.Value.Date);
                Vm.AvgDate = e.NewDate.Value.Date;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void AvgList_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var listView = sender as ListView;
            var selectedItem = listView?.SelectedItem as ExchangeRate;
            if (selectedItem == null) return;

            if (!Equals(Vm.HisCurrency, selectedItem.Currency)
     && _historyPivotBackup == null)
            {
                _historyDrawn = false;
                                Vm.HisEr = null;
//                if (HisChart.Series.Count > 0) HisChart.Series.RemoveAt(0);
            }

            // restore history pivot
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

        private async void HisDraw_OnClick(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            Vm.ChangesEnabled = false;
            Vm.HisDateBackup();

            if ((Vm.HisDateFrom == null) || (Vm.HisDateTo == null))
            {
                return;
            }

            using (var h = _providerHelper.Helper())
            {
                await h.InitCache();

                var ers = new ObservableCollection<ExchangeRate>();
                await h.ErService.GetExchangeRateHistory(
                    Vm.HisCurrency, Vm.HisDateFrom.Value.Date, Vm.HisDateTo.Value.Date,
                    ers, h.Progress);
                Vm.HisEr = ers;
//                HisChart.UpdateLayout();
//                await HisChart.WaitForLayoutUpdateAsync();

                await h.FlushCache();
            }

            Vm.ChangesEnabled = true;
            _historyDrawn = true;
            Vm.HisSaveEnabled = true;

#if DEBUG
            DebugElapsedTime(sw, nameof(HisDraw_OnClick));
#endif
        }

        private void HisDateFromPicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs e)
        {
            if (e.NewDate != null)
            {
                Vm.HisDateFrom = e.NewDate.Value.Date;
            }
        }

        private void HitDateToPicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs e)
        {
            if (e.NewDate != null)
            {
                Vm.HisDateTo = e.NewDate.Value.Date;
            }
        }

        private async void HisSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Vm.ChangesEnabled = false;

            var suggestedName = string.Format("{0}_{1}{2}",
                Vm.HisCurrency.Code,
                Vm.HisDateFrom?.ToString("ddMMyy") ?? "?",
                Vm.HisDateTo?.ToString("ddMMyy") ?? "?");

            using (var h = new UiElementToPngHelper(HisChart, suggestedName, _progressSubscriber))
            {
                await h.Execute();
            }

            Vm.ChangesEnabled = true;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.HisSaveEnabled = (MainPivot.SelectedIndex == 1) && _historyDrawn;
        }

        private void OnBackRequested(object s, BackRequestedEventArgs e)
        {
            e.Handled = MainPivot.SelectedIndex > 0;

            // ReSharper disable once InvertIf
            if (MainPivot.SelectedIndex > 0)
            {
                MainPivot.SelectedIndex = MainPivot.SelectedIndex - 1;
                Vm.HisDateRecover();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private static void DebugElapsedTime(Stopwatch sw, string methodName)
        {
            sw.Stop();
            Debug.WriteLine("{0}-time: {1}", methodName, sw.Elapsed.ToString("mm':'ss':'fff"));
        }
    }
}