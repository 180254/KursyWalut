using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Cimbalino.Toolkit.Extensions;
using KursyWalut.Helper;
using KursyWalut.Model;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KursyWalut.Page
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly PropertySetHelper _localSettings;
        private readonly ResourceLoader _resLoader;
        private readonly double _scaleFactor;

        private readonly EventHandler<int> _progressSubscriber;
        private readonly ProviderHelper _providerHelper;

        private object _historyPivotBackup;
        private bool _historyDrawn;

        public MainPage()
        {
            Vm = new MainPageVm();
            _localSettings = new PropertySetHelper(ApplicationData.Current.LocalSettings.Values);

            InitializeComponent();
            Application.Current.Suspending += OnSuspending;
            Application.Current.UnhandledException += OnUnhandledException;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            _resLoader = ResourceLoader.GetForCurrentView();
            _scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

            // initialize provider helper
            var cache = CacheHelper.GetStandardLsc();
            _progressSubscriber = (sender, i) => Vm.Progress = i;
            _providerHelper = new ProviderHelper(cache, Vm.ProgressMax, _progressSubscriber);

            // ReSharper disable once InvertIf
            if (_localSettings.GetValue<string>(nameof(Vm.HisCurrency)) == null)
            {
                // remove "history" pivot
                _historyPivotBackup = MainPivot.Items?[1];
                MainPivot.Items?.RemoveAt(1);
            }
        }

        public MainPageVm Vm { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await AvgInit();
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var historyPivotSelected = MainPivot.SelectedIndex == 1;
            Vm.HisSaveEnabled = historyPivotSelected && _historyDrawn;
            Vm.RotatePivotForegrounds();
        }

        // ---------------------------------------------------------------------------------------------------------------

        private async void AvgRetryInitButton_OnClick(object sender, RoutedEventArgs e)
        {
            Vm.InitDone = true;
            await AvgInit();
        }

        // ---------------------------------------------------------------------------------------------------------------

        private async Task AvgInit()
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
                Vm.AvgDate = _localSettings.GetValue(nameof(Vm.AvgDate), lastAvailableDay);
                Vm.HisDateFrom = _localSettings.GetValue(nameof(Vm.HisDateFrom), lastAvailableDay.AddYears(-1));
                Vm.HisDateTo = _localSettings.GetValue(nameof(Vm.HisDateTo), lastAvailableDay);
                Vm.HisDateToMax = lastAvailableDay;

                var hisCurrencyCode = _localSettings.GetValue<string>(nameof(Vm.HisCurrency));
                if (hisCurrencyCode != null)
                    Vm.HisCurrency = Currency.DummyForCode(hisCurrencyCode);

                MainPivot.SelectedIndex = _localSettings.GetValue(nameof(MainPivot), 0);

                var erProgress = h.Progress.SubPercent(0.10, 0.50);
                Vm.AvgErList = await h.ErService.GetExchangeRates(lastAvailableDay, erProgress);

                var availProgress = h.Progress.SubPercent(0.50, 1.00);
                Vm.AvailDates = await h.ErService.GetAllAvailablesDay(availProgress);

                await h.FlushCache();
            }

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

        private void HisDateFromPicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs e)
        {
            if ((e.NewDate != null) && (e.NewDate?.Date != e.OldDate?.Date))
            {
                Vm.HisDateFrom = e.NewDate.Value.Date;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void HitDateToPicker_OnDateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs e)
        {
            if ((e.NewDate != null) && (e.NewDate?.Date != e.OldDate?.Date))
            {
                Vm.HisDateTo = e.NewDate.Value.Date;
            }
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

            var ers = new List<ExchangeRate>();
            using (var h = _providerHelper.Helper())
            {
                await h.InitCache();

                var expectedSize = (int) (HisChart.ActualWidth*_scaleFactor*1.1);
                await h.ErService.GetExchangeRateAveragedHistory(
                    Vm.HisCurrency, Vm.HisDateFrom.Value, Vm.HisDateTo.Value,
                    ers, expectedSize, h.Progress);
                Vm.HisErList = ers;

                Debug.WriteLine("{0}-width={1};expectedSize={2};gotSize={3}",
                    nameof(HisDraw_OnClick), (int) HisChart.ActualWidth, expectedSize, ers.Count);

                await h.FlushCache();
            }

            if (ers.IsEmpty())
            {
                await new MessageDialog(_resLoader.GetString("NoSuchHistory/Text")).ShowAsync();
            }

            _historyDrawn = ers.Count > 0;
            Vm.ChangesEnabled = true;
            Vm.HisSaveEnabled = true;
            Vm.AllDatesBackup();

#if DEBUG
            DebugElapsedTime(sw, nameof(HisDraw_OnClick));
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        private async void HisSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var suggestedName = string.Format("{0}_{1}-{2}",
                Vm.HisCurrency.Code,
                Vm.HisDateFrom?.ToString("ddMMyy") ?? "?",
                Vm.HisDateTo?.ToString("ddMMyy") ?? "?");

            Vm.ChangesEnabled = false;

            using (var h = new UiToPngFileHelper(HisChart, suggestedName, Vm.ProgressMax, _progressSubscriber))
            {
                var saved = await h.Execute();

                if (saved)
                {
                    Vm.BottomAppBarIsOpen = false;
                    await new MessageDialog(_resLoader.GetString("HisSaveConfirmation/Text")).ShowAsync();
                }
            }

            Vm.ChangesEnabled = true;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            Debug.WriteLine("Suspending({0})", typeof (MainPage));
            _localSettings[nameof(MainPivot)] = MainPivot.SelectedIndex;
            _localSettings[nameof(Vm.AvgDate)] = Vm.AvgDate;
            _localSettings[nameof(Vm.HisDateFrom)] = Vm.HisDateFrom;
            _localSettings[nameof(Vm.HisDateTo)] = Vm.HisDateTo;
            _localSettings[nameof(Vm.HisCurrency)] = Vm.HisCurrency?.Code;
            _localSettings[nameof(_historyDrawn)] = _historyDrawn;

            deferral.Complete();
        }

        // ---------------------------------------------------------------------------------------------------------------

        private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("{0}: {1}", nameof(OnUnhandledException), e.Message);
            e.Handled = true;

            Vm.Progress = 0;
            var dialogTextId = e.Exception is IOException ? "NoInternet/Text" : "OtherException/Text";
            await new MessageDialog(_resLoader.GetString(dialogTextId)).ShowAsync();

            AvgList.SelectedItem = null;

            var initSuccessfully = Vm.AvailDates != null;
            if (!initSuccessfully)
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

        private static void DebugElapsedTime(Stopwatch sw, string methodName)
        {
            sw.Stop();
            Debug.WriteLine("{0}-time: {1}", methodName, sw.Elapsed.ToString("mm':'ss':'fff"));
        }

        private async void CacheAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            Vm.ChangesEnabled = false;
            Vm.BottomAppBarIsOpen = false;

            var ers = new List<ExchangeRate>();
            using (var h = _providerHelper.Helper())
            {
                await h.InitCache();

                var firstProgress = h.Progress.SubPercent(0.00, 0.01);
                var firstAvailableDay = await h.ErService.GetFirstAvailableDay(firstProgress);

                var lastProgress = h.Progress.SubPercent(0.01, 0.02);
                var lastAvailableDay = await h.ErService.GetLastAvailableDay(lastProgress);

                var avgProgress = h.Progress.SubPercent(0.02, 0.05);
                Vm.AvgErList = await h.ErService.GetExchangeRates(lastAvailableDay, avgProgress);

                Vm.HisDateFrom = lastAvailableDay.AddYears(-1);
                Vm.HisDateTo = lastAvailableDay;
                Vm.HisErList = new List<ExchangeRate>();
                _historyDrawn = false;

                var hisProgress = h.Progress.SubPercent(0.05, 1.00);
                await h.ErService.GetExchangeRateAveragedHistory(
                    Currency.DummyForCode("USD"), firstAvailableDay, lastAvailableDay,
                    ers, int.MaxValue, hisProgress);

                Debug.WriteLine("CacheAll-{0}", ers.Count);

                await h.FlushCache();
            }

            var msgString = string.Format("{0} [{1}].", _resLoader.GetString("CacheAllInfo/Text"), ers.Count);
            await new MessageDialog(msgString).ShowAsync();

            Vm.AllDatesBackup();
            Vm.ChangesEnabled = true;
        }
    }
}