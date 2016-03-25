using System;
using System.Collections;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using KursyWalut.Cache;
using KursyWalut.Model;
using KursyWalut.Progress;
using KursyWalut.ProviderImpl;
using Microsoft.ApplicationInsights;

namespace KursyWalut
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync(
                WindowsCollectors.Metadata |
                WindowsCollectors.Session);
            InitializeComponent();
            Suspending += OnSuspending;



//            //            var cache = new InMemCache();
//            var cache = LocalStorageCache.GetStandard();
//
//            var nbpPr = new NbpExchangeRatesProvider(cache, PProgress.NewMaster());
//            var cachePr = new CacheExchangeRateProvider(nbpPr, cache, PProgress.NewMaster());
//            var nbpSe = new StandardExchangeRateService(cachePr);
//            var prog = PProgress.NewMaster();
//
//
//            Debug.WriteLine("The number of processors on this computer is {0}.",
//                Environment.ProcessorCount);
//
//            var x = nbpSe.GetAvailableDays(2016, prog);
//            x.Wait();
//
//            Debug.WriteLine("-----------------------------------------------------------(A0)");
//            foreach (var dateTime in x.Result)
//            {
//                Debug.WriteLine(dateTime);
//            }
//            Debug.WriteLine("-----------------------------------------------------------(A1)");
//
//
//            var r = nbpSe.GetExchangeRates(x.Result[0], prog);
//            r.Wait();
//
//            Debug.WriteLine("-----------------------------------------------------------(B0)");
//            foreach (var exchangeRate in r.Result)
//            {
//                Debug.WriteLine(exchangeRate);
//            }
//            Debug.WriteLine("-----------------------------------------------------------(B1)");
//
//
//            var first = nbpSe.GetFirstAvailableDay(prog);
//            first.Wait();
//            var last = nbpSe.GetLastAvailableDay(prog);
//            last.Wait();
//
//
//
//
//
//
//
//
//
//
//
//
//
//            var stopwatch = new Stopwatch();
//            stopwatch.Start();
//
//
//            prog = PProgress.NewMaster();
//            var progresses = ArrayList.Synchronized(new ArrayList());
//
//            prog.ProgressChanged += (sender, changed) => progresses.Add(changed);
//
//            var start11 = last.Result.Subtract(TimeSpan.FromDays(365*5));
////            var start11 = first.Result;
//
//            var stopp11 = last.Result;
//            var currency = new Currency("USD", "", 1);
//
//            Debug.WriteLine("->" + start11 + "/" + stopp11);
//            var exchangeRatesHistory = nbpSe.GetExchangeRateHistory(currency, start11, stopp11,
//                prog);
//            exchangeRatesHistory.Wait();
//
//
//            Debug.WriteLine("TIME(0) " + stopwatch.Elapsed);
//
//            prog.ProgressChanged += (sender, changed) => Debug.WriteLine("E-"+changed);
//
//
//            nbpSe.FlushCache(PProgress.NewMaster());
//
//            Debug.WriteLine("TIME(1) " + stopwatch.Elapsed);
//
//
//            Debug.WriteLine("P-" + progresses.Count + "-" + string.Join(",", progresses.ToArray()));
//
//       
//
//            Debug.WriteLine("-----------------------------------------------------------(C0)");
//            foreach (var exchangeRate in exchangeRatesHistory.Result)
//            {
////                Debug.WriteLine(exchangeRate);
//            }
//
//            Debug.WriteLine("-----------------------------------------------------------(C1)");
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//            prog = PProgress.NewMaster();
//            progresses = ArrayList.Synchronized(new ArrayList());
//            prog.ProgressChanged += (sender, changed) => progresses.Add(changed);
//
//            stopwatch.Reset();
//            var exchangeRatesHistor2y = nbpSe.GetExchangeRateHistory(currency, start11, stopp11,
//                prog.SubPercent(0.00, 0.80));
//            exchangeRatesHistor2y.Wait();
//            nbpSe.FlushCache(prog.SubPercent(0.80, 1.00));
//
//            Debug.WriteLine("TIME " + stopwatch.Elapsed);
//
//            stopwatch.Stop();
//
//            Debug.WriteLine("P-" + progresses.Count + "-" + string.Join(",", progresses.ToArray()));
//
//            Debug.WriteLine("-----------------------------------------------------------(D0)");
//
//            Debug.WriteLine("TIME " + stopwatch.Elapsed);
//            Debug.WriteLine("-----------------------------------------------------------(D1)");
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
//                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof (MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}